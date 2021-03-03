using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MimicApi.Database;
using MimicApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MimicApi.Controllers
{
    [Route("api/palavras")]
    public class PalavrasController : ControllerBase
    {
        private readonly MimicContext _banco;
        public PalavrasController(MimicContext banco)
        {
            _banco = banco;
        }

        //App -- /api/palavras?data=2021-03-03
        [Route("")]
        [HttpGet]
        public ActionResult ObterTodas(DateTime? data, int? paginaNumero, int? qtdItens)
        {
            var item = _banco.Palavras.AsQueryable();
            if (data.HasValue)
            {
                item = item.Where(b => b.Criado > data.Value || b.Atualizado > data.Value);
            }

            if (paginaNumero.HasValue)
            {
                //Lógica de paginação
                item = item.Skip((paginaNumero.Value - 1) * qtdItens.Value).Take(qtdItens.Value);
            }

            return Ok(item);
        }

        //Web -- /api/palavras/1
        [Route("{id}")]
        [HttpGet]
        public ActionResult Obter(int id)
        {
            var palavra = _banco.Palavras.Find(id);
            if(palavra == null)
            {
                //return StatusCode(404);
                return NotFound();
            }

            return Ok(palavra);
        }

        //-- /api/palavras(POST: id, nome, pontuacao)
        [Route("")]
        [HttpPost]
        public ActionResult Cadastrar([FromBody] Palavra palavra)
        {
            _banco.Palavras.Add(palavra);
            _banco.SaveChanges();
            return Created($"/api/palavras/{palavra.Id}", palavra);
        }

        //-- /api/palavras/1 (PUT: id, nome, pontuacao, ativo, criacao)
        [Route("{id}")]
        [HttpPut]
        public ActionResult Atualizar(int id, [FromBody] Palavra palavra)
        {
            var obj = _banco.Palavras.AsNoTracking().FirstOrDefault(b=>b.Id == id);
            if (obj == null)
            {
                //return StatusCode(404);
                return NotFound();
            }

            palavra.Id = id;
            _banco.Palavras.Update(palavra);
            _banco.SaveChanges();
            return Ok();
        }

        //-- /api/palavras/1
        [Route("{id}")]
        [HttpDelete]
        public ActionResult Deletar(int id)
        {
            var palavra = _banco.Palavras.Find(id);
            if (palavra == null)
            {
                //return StatusCode(404);
                return NotFound();
            }

            palavra.Ativo = false;
            _banco.Palavras.Update(palavra);
            _banco.SaveChanges();
            return NoContent();
        }
    }
}
