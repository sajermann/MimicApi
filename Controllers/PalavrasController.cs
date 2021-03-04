using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using MimicApi.Helpers;
using MimicApi.Models;
using MimicApi.Models.DTO;
using MimicApi.Repositories.Contracts;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace MimicApi.Controllers
{
    [Route("api/palavras")]
    public class PalavrasController : ControllerBase
    {
        private readonly IPalavraRepository _repository;
        private readonly IMapper _mapper;
        public PalavrasController(IPalavraRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        //App -- /api/palavras?data=2021-03-03
        [HttpGet("", Name = "ObterTodas")]
        public ActionResult ObterTodas([FromQuery] PalavraUrlQuery palavraUrlQuery)
        {
            var item = _repository.ObterPalavra(palavraUrlQuery);

            if (item.Results.Count == 0)
            {
                return NotFound();
            }

            if(item.Paginacao != null)
            {
                Response.Headers.Add("X-Pagination", JsonConvert.SerializeObject(item.Paginacao));
            }

            var lista = _mapper.Map<PaginationList<Palavra>, PaginationList<PalavraDTO>>(item);

            foreach(var palavra in lista.Results)
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

            return Ok(lista);
        }


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
            palavraDTO.Links = new List<LinkDTO>();
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

        //-- /api/palavras(POST: id, nome, pontuacao)
        [HttpPost]
        public ActionResult Cadastrar([FromBody] Palavra palavra)
        {
            _repository.Cadastrar(palavra);
            
            return Created($"/api/palavras/{palavra.Id}", palavra);
        }

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

            palavra.Id = id;
            _repository.Atualizar(palavra);
           return Ok();
        }

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
