using Alura.ListaLeitura.App.Negocio;
using Alura.ListaLeitura.App.Repositorio;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Alura.ListaLeitura.App
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddRouting();
        }

        public void Configure(IApplicationBuilder app)
        {
            var routeBuilder = new RouteBuilder(app);
            routeBuilder.MapRoute("Livros/ParaLer", LivrosParaLer);
            routeBuilder.MapRoute("Livros/Lidos", LivrosLidos);
            routeBuilder.MapRoute("Livros/Lendo", LivrosLendo);
            routeBuilder.MapRoute("cadastro/novolivro/{nome}/{autor}", CadastroNovoLivro);
            routeBuilder.MapRoute("Detalhes/{id:int}", ExibirDetalhes);
            routeBuilder.MapRoute("cadastro/novolivro/", ExibirFormulario);
            routeBuilder.MapRoute("cadastro/incluir", ProcessarFormulario);

            var rotas = routeBuilder.Build();

            app.UseRouter(rotas);

            //app.Run(Roteamento);
        }

        public Task ProcessarFormulario(HttpContext context)
        {
            var livro = new Livro()
            {                
                //Titulo = context.Request.Query["nome"],
                //Autor = context.Request.Query["autor"],
                Titulo = context.Request.Form["nome"],
                Autor = context.Request.Form["autor"],
            };

            var _repo = new LivroRepositorioCSV();
            _repo.Incluir(livro);
            return context.Response.WriteAsync("LIVRO ADICIONADO COM SUCESSO!");
        }

        public Task ExibirFormulario(HttpContext context)
        {
            var formulario = CarregarArquivoHtml("formulario");

            return context.Response.WriteAsync(formulario);
        }

        private string CarregarArquivoHtml(string nomeDoArquivo)
        {
            //var html = @"<html>
            //    <form action='cadastro/incluir'>
            //        <input name='titulo' />
            //        <input name='autor' />
            //        <button>Gravar</button>
            //    </form>
            //</html>";

            var nomeCompletoDoArquivo = $"../../../HTML/{nomeDoArquivo}.html"; //html separado em outro arquivo
            using (var arquivo = File.OpenText(nomeCompletoDoArquivo)) { 
                return arquivo.ReadToEnd();
            }
        }

        public Task ExibirDetalhes(HttpContext context)
        {
            var id = Convert.ToInt32(context.GetRouteValue("id"));
            var _repo = new LivroRepositorioCSV();
            var livro = _repo.Todos.First(l => l.Id == id);

            return context.Response.WriteAsync(livro.Detalhes());
        }

        public Task CadastroNovoLivro(HttpContext context)
        {
            var livro = new Livro()
            {
                Titulo = context.GetRouteValue("nome").ToString(),
                Autor = context.GetRouteValue("autor").ToString(),
            };

            var _repo = new LivroRepositorioCSV();
            _repo.Incluir(livro);
            return context.Response.WriteAsync("LIVRO ADICIONADO COM SUCESSO!");
        }

        public Task Roteamento(HttpContext context)  //roteamento rudimentar
        {
            var _repo = new LivroRepositorioCSV();

            var caminhosAtendidos = new Dictionary<string, RequestDelegate>
            {
                {"/Livros/ParaLer", LivrosParaLer },
                {"/Livros/Lidos", LivrosLidos },
                {"/Livros/Lendo", LivrosLendo }
            };

            if (caminhosAtendidos.ContainsKey(context.Request.Path)) //verifica se o caminho existe como chave dentro do dictionary
            {
                var metodo = caminhosAtendidos[context.Request.Path];
                return metodo.Invoke(context);
            }

            context.Response.StatusCode = 404;
            return context.Response.WriteAsync("Caminho não existe.");
        }

        public Task LivrosParaLer(HttpContext context)
        {
            var _repo = new LivroRepositorioCSV();

            var conteudo = GerarHTML("para-ler", _repo.ParaLer.Livros);

            return context.Response.WriteAsync(conteudo);

            //return context.Response.WriteAsync(_repo.ParaLer.ToString());
        }

        public Task LivrosLendo(HttpContext context)
        {
            var _repo = new LivroRepositorioCSV();

            var conteudo = GerarHTML("lendo", _repo.Lendo.Livros);

            return context.Response.WriteAsync(conteudo);
        }

        public Task LivrosLidos(HttpContext context)
        {
            var _repo = new LivroRepositorioCSV();

            var conteudo = GerarHTML("lidos", _repo.Lidos.Livros);

            return context.Response.WriteAsync(conteudo);
        }

        public string GerarHTML(string arquivo, IEnumerable<Livro> lista)
        {
            var conteudoArquivo = CarregarArquivoHtml(arquivo);
            foreach (var livro in lista)
            {
                conteudoArquivo = conteudoArquivo.Replace("#NOVO-ITEM#", $"<li>{livro.Titulo} - {livro.Autor}</li>#NOVO-ITEM#");
            }
            conteudoArquivo = conteudoArquivo.Replace("#NOVO-ITEM#", "");
            return conteudoArquivo;
        }
    }
}