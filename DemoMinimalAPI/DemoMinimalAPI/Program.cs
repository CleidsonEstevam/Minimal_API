using DemoMinimalAPI.Data;
using DemoMinimalAPI.Models;
using Microsoft.EntityFrameworkCore;
using MiniValidation;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

#region "Mapeando ConnectionString"
builder.Services.AddDbContext<MinimalContextDb>(options =>
        options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

#endregion

//Requests
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

#region "Actions"
//Rota=>Ação
app.MapGet("/fornecedor", async (
    MinimalContextDb context) =>
    await context.Fornecedores.ToListAsync())
    .WithName("GetFornecedores")
    .WithTags("Fornecedores");


//Rota=>Parametro=>Ação=>Validação=>Retorno
app.MapGet("/fornecedor/{id}", async (
    Guid id,
    MinimalContextDb context) =>

    await context.Fornecedores.FindAsync(id)
          is Fornecedor fornecedor
                ? Results.Ok(fornecedor)
                : Results.NotFound())
    //O que é produzido
    .Produces<Fornecedor>(StatusCodes.Status200OK)
    .Produces(StatusCodes.Status404NotFound)
    .WithName("GetFornecedorPorId")
    .WithTags("Fornecedor");



app.MapPost("/fornecedor", async (
    MinimalContextDb context,
    Fornecedor fornecedor) =>
{
    if (!MiniValidator.TryValidate(fornecedor, out var errors))
        return Results.ValidationProblem(errors);

    context.Fornecedores.Add(fornecedor);
    var result = await context.SaveChangesAsync();

    return result > 0
    ? Results.Created($"/fornecedor/{fornecedor.Id}", fornecedor)
    : Results.BadRequest("Problema ao salvar registro.");

})
    .Produces<Fornecedor>(StatusCodes.Status201Created)
    .Produces(StatusCodes.Status404NotFound)
    .WithName("PostFornecedorPorId")
    .WithTags("Fornecedor");


app.MapPut("/fornecedor/{id}", async (
    Guid id,
    MinimalContextDb context,
    Fornecedor fornecedor) =>
{
    var fornecedorBanco = await context.Fornecedores.FindAsync(id);
    if (fornecedorBanco == null) return Results.NotFound();

    if (!MiniValidator.TryValidate(fornecedor, out var errors))
        return Results.ValidationProblem(errors);

    context.Fornecedores.Update(fornecedor);
    var result = await context.SaveChangesAsync();

    return result > 0
            ? Results.NoContent()
            : Results.BadRequest("Houve um problema ao salvar o registro");

}).ProducesValidationProblem()
    .Produces(StatusCodes.Status204NoContent)
    .Produces(StatusCodes.Status400BadRequest)
    .WithName("PutFornecedor")
    .WithTags("Fornecedor");


app.MapDelete("/fornecedor/{id}", async (
    Guid id,
    MinimalContextDb context) =>
{
    var fornecedor = await context.Fornecedores.FindAsync(id);
    if (fornecedor == null) return Results.NotFound();

    context.Remove(fornecedor);
    var result = await context.SaveChangesAsync();

    return result > 0
         ? Results.NoContent()
         : Results.BadRequest("Houve um problema ao delear o registro");


}).Produces(StatusCodes.Status400BadRequest)
    .Produces(StatusCodes.Status204NoContent)
    .Produces(StatusCodes.Status404NotFound)
    .RequireAuthorization("ExcluirFornecedor")
    .WithName("DeleteFornecedor")
    .WithTags("Fornecedor");

#endregion

app.Run();

