using ApiCatalogo.Context;
using ApiCatalogo.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ApiCatalogo.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class ProdutosController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ProdutosController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Produto>>> GetProdutos()
        {
            var produtos = await _context.Produtos.ToListAsync();

            if (!produtos.Any())
            {
                return NotFound();
            }

            return Ok(produtos);
        }

        [HttpGet]
        public async Task<ActionResult<Produto>> GetProdutoById(int id)
        {
            var produto = await _context.Produtos
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == id);

            if (produto == null)
            {
                return NotFound();
            }

            return Ok(produto);
        }

        [HttpPost]
        public async Task<ActionResult<Produto>> PostCreateProduto(Produto produto)
        {

            if (produto is null)
                return BadRequest();

            var categoriaExiste = await _context.Categorias
                .AnyAsync(c => c.Id == produto.CategoriaId);

            if (!categoriaExiste)
                return BadRequest("Categoria não encontrada");

            _context.Produtos.Add(produto);

            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetProdutoById),
                new { id = produto.Id },
                produto
            );
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult<Produto>> PutUpdateProduto(int id, Produto produto)
        {
            if (id != produto.Id)
                return BadRequest("Os IDs não conferem");

            var categoriaExiste = await _context.Categorias
                .AnyAsync(c => c.Id == produto.CategoriaId);

            if (!categoriaExiste)
                return BadRequest("Categoria não encontrada");

            _context.Entry(produto).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _context.Produtos.AnyAsync(p => p.Id == id))
                {
                    return NotFound("O produto não existe mais no banco de dados");
                }
                else
                {
                    throw;
                }
            }
            return Ok(produto);
        }

        [HttpDelete("{id:int}")]
        public async Task<ActionResult<Produto>> DeleteProduto(int id)
        {
            var produtoDeletado = await _context.Produtos.FindAsync(id);
            if (produtoDeletado == null)
                return NotFound("Produto não encontrado");

            _context.Produtos.Remove(produtoDeletado);

            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}