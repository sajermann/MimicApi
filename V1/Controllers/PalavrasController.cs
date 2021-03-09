using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using MimicApi.Helpers;
using MimicApi.V1.Models;
using MimicApi.V1.Models.DTO;
using MimicApi.V1.Repositories.Contracts;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MimicApi.V1.Controllers
{
    [ApiController]
    [Route("api/v{version:apiVersion}/[controller]")]
    //[Route("api/[controller]")]
    [ApiVersion("1.0", Deprecated = true)]
    [ApiVersion("1.1")]
    public class PalavrasController : ControllerBase
    {
        private readonly IPalavraRepository _repository;
        private readonly IMapper _mapper;
        public PalavrasController(IPalavraRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        /// <summary>
        /// Operação que pega do banco de dados todas as palavras existentes
        /// </summary>
        /// <param name="palavraUrlQuery">Filtros de pesquisa</param>
        /// <returns>Listagem de palavras</returns>
        [MapToApiVersion("1.0")]
        [MapToApiVersion("1.1")]
        //App -- /api/palavras?data=2021-03-03
        [HttpGet("", Name = "ObterTodas")]
        public ActionResult ObterTodas([FromQuery] PalavraUrlQuery palavraUrlQuery)
        {
            var item = _repository.ObterPalavra(palavraUrlQuery);

            if (item.Results.Count == 0)
            {
                return NotFound();
            }

            PaginationList<PalavraDTO> lista = CriarLinksListPalavraDTO(palavraUrlQuery, item);

            return Ok(lista);
        }

        private PaginationList<PalavraDTO> CriarLinksListPalavraDTO(PalavraUrlQuery palavraUrlQuery, PaginationList<Palavra> item)
        {
            var lista = _mapper.Map<PaginationList<Palavra>, PaginationList<PalavraDTO>>(item);

            foreach (var palavra in lista.Results)
            {
                palavra.Links = new List<LinkDTO>();
                palavra.Links.Add(new LinkDTO(
                    "self",
                    Url.Link("ObterPalavra", new { id = palavra.Id }),
                    "GET"));
            }

            lista.Links.Add(new LinkDTO(
                    "self",
                    Url.Link("ObterTodas", palavraUrlQuery),
                    "GET"));

            if (item.Paginacao != null)
            {
                Response.Headers.Add("X-Pagination", JsonConvert.SerializeObject(item.Paginacao));

                if (palavraUrlQuery.PaginaNumero + 1 <= item.Paginacao.TotalPaginas)
                {
                    var queryString = new PalavraUrlQuery() { PaginaNumero = palavraUrlQuery.PaginaNumero + 1, PagRegistro = palavraUrlQuery.PagRegistro, Data = palavraUrlQuery.Data };
                    lista.Links.Add(new LinkDTO(
                        "next",
                        Url.Link("ObterTodas", queryString),
                        "GET"));
                }
                if (palavraUrlQuery.PaginaNumero - 1 > 0)
                {
                    var queryString = new PalavraUrlQuery() { PaginaNumero = palavraUrlQuery.PaginaNumero - 1, PagRegistro = palavraUrlQuery.PagRegistro, Data = palavraUrlQuery.Data };
                    lista.Links.Add(new LinkDTO(
                    "prev",
                    Url.Link("ObterTodas", queryString),
                    "GET"));
                }
            }

            return lista;
        }

        [MapToApiVersion("1.0")]
        [MapToApiVersion("1.1")]
        //Web -- /api/palavras/1
        [HttpGet("{id}", Name = "ObterPalavra")]
        public ActionResult Obter(int id)
        {
            var palavra = _repository.Obter(id);
            if(palavra == null)
            {
                //return StatusCode(404);
                return NotFound();
            }

            PalavraDTO palavraDTO = _mapper.Map<Palavra, PalavraDTO>(palavra);
            palavraDTO.Links.Add(
                new LinkDTO(
                    "self", 
                    Url.Link("ObterPalavra", new { id = palavraDTO.Id}),
                    "GET")
                );

            palavraDTO.Links.Add(
            new LinkDTO(
                "update",
                Url.Link("AtualizarPalavra", new { id = palavraDTO.Id }),
                "PUT")
            );
            palavraDTO.Links.Add(
            new LinkDTO(
                "delete",
                Url.Link("ExcluirPalavra", new { id = palavraDTO.Id }),
                "DELETE")
            );

            return Ok(palavraDTO);
        }

        [MapToApiVersion("1.0")]
        [MapToApiVersion("1.1")]
        //-- /api/palavras(POST: id, nome, pontuacao)
        [HttpPost]
        public ActionResult Cadastrar([FromBody] Palavra palavra)
        {
            if (palavra == null)
            {
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                return UnprocessableEntity(ModelState);
            }
            
            
            palavra.Criado = DateTime.Now;
            palavra.Ativo = true;
            _repository.Cadastrar(palavra);

            PalavraDTO palavraDTO = _mapper.Map<Palavra, PalavraDTO>(palavra);
            palavraDTO.Links.Add(
                new LinkDTO(
                    "self",
                    Url.Link("ObterPalavra", new { id = palavraDTO.Id }),
                    "GET")
                );

            return Created($"/api/palavras/{palavra.Id}", palavraDTO);
        }

        [MapToApiVersion("1.0")]
        [MapToApiVersion("1.1")]
        //-- /api/palavras/1 (PUT: id, nome, pontuacao, ativo, criacao)
        [HttpPut("{id}", Name = "AtualizarPalavra")]
        public ActionResult Atualizar(int id, [FromBody] Palavra palavra)
        {
            var obj = _repository.Obter(id);
            if (obj == null)
            {
                //return StatusCode(404);
                return NotFound();
            }

            if (palavra == null)
            {
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                return UnprocessableEntity(ModelState);
            }

            palavra.Id = id;
            palavra.Ativo = obj.Ativo;
            palavra.Criado = obj.Criado;
            palavra.Atualizado = DateTime.Now;

            _repository.Atualizar(palavra);

            PalavraDTO palavraDTO = _mapper.Map<Palavra, PalavraDTO>(palavra);
            palavraDTO.Links.Add(
                new LinkDTO(
                    "self",
                    Url.Link("ObterPalavra", new { id = palavraDTO.Id }),
                    "GET")
                );

            return Ok();
        }

        [MapToApiVersion("1.1")]
        //-- /api/palavras/1
        [HttpDelete("{id}", Name = "ExcluirPalavra")]
        public ActionResult Deletar(int id)
        {
            var palavra = _repository.Obter(id);
            if (palavra == null)
            {
                //return StatusCode(404);
                return NotFound();
            }
            _repository.Deletar(id);
            return NoContent();
        }
    }
}
