using Microsoft.EntityFrameworkCore;
using MimicApi.Database;
using MimicApi.Helpers;
using MimicApi.Models;
using MimicApi.Repositories.Contracts;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MimicApi.Repositories
{
    public class PalavraRepository : IPalavraRepository
    {
        private readonly MimicContext _banco;
        public PalavraRepository(MimicContext banco)
        {
            _banco = banco;
        }
        public PaginationList<Palavra> ObterPalavra(PalavraUrlQuery query)
        {
            var lista = new PaginationList<Palavra>();
            var item = _banco.Palavras.AsNoTracking().AsQueryable();
            if (query.Data.HasValue)
            {
                item = item.Where(b => b.Criado > query.Data.Value || b.Atualizado > query.Data.Value);
            }

            if (query.PaginaNumero.HasValue)
            {
                var quatidadeTotalRegistro = item.Count();

                //Lógica de paginação
                item = item.Skip((query.PaginaNumero.Value - 1) * query.PagRegistro.Value).Take(query.PagRegistro.Value);


                var paginacao = new Paginacao();
                paginacao.NumeroPagina = query.PaginaNumero.Value;
                paginacao.RegistroPorPagina = query.PagRegistro.Value;
                paginacao.TotalRegistro = quatidadeTotalRegistro;
                paginacao.TotalPaginas = (int)Math.Ceiling((double)quatidadeTotalRegistro / query.PagRegistro.Value);
                lista.Paginacao = paginacao;
            }

            lista.AddRange(item.ToList());

            return lista;
        }

        public Palavra Obter(int id)
        {
            return _banco.Palavras.AsNoTracking().FirstOrDefault(b => b.Id == id);
        }
        public void Cadastrar(Palavra palavra)
        {
            _banco.Palavras.Add(palavra);
            _banco.SaveChanges();
        }
        public void Atualizar(Palavra palavra)
        {
            _banco.Palavras.Update(palavra);
            _banco.SaveChanges();
        }

        public void Deletar(int id)
        {
            var palavra = Obter(id);
            palavra.Ativo = false;
            _banco.Palavras.Update(palavra);
            _banco.SaveChanges();
        }




    }
}
