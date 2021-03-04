using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MimicApi.Database;
using MimicApi.Helpers;
using MimicApi.Models;
using MimicApi.Models.DTO;
using MimicApi.Repositories.Contracts;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
        [Route("")]
        [HttpGet]
        public ActionResult ObterTodas([FromQuery] PalavraUrlQuery palavraUrlQuery)
        {
            var item = _repository.ObterPalavra(palavraUrlQuery);

            if (palavraUrlQuery.PaginaNumero > item.Paginacao.TotalPaginas)
            {
                return NotFound();
            }
            Response.Headers.Add("X-Pagination", JsonConvert.SerializeObject(item.Paginacao));
            return Ok(item.ToList());
        }


        //Web -- /api/palavras/1
        [Route("{id}")]
        [HttpGet]
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
                    $"/api/palavras/{palavraDTO.Id}",
                        "GET")
                );

            return Ok(palavraDTO);
        }

        //-- /api/palavras(POST: id, nome, pontuacao)
        [Route("")]
        [HttpPost]
        public ActionResult Cadastrar([FromBody] Palavra palavra)
        {
            _repository.Cadastrar(palavra);
            
            return Created($"/api/palavras/{palavra.Id}", palavra);
        }

        //-- /api/palavras/1 (PUT: id, nome, pontuacao, ativo, criacao)
        [Route("{id}")]
        [HttpPut]
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
        [Route("{id}")]
        [HttpDelete]
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
