using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Adicionando o DbContext em memória
builder.Services.AddDbContext<AnimalContext>(opt => opt.UseInMemoryDatabase("AnimalsDb"));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGet("/", () => "Welcome to PetsApi");

app.MapGet("/animals", async (AnimalContext db) => await db.Animals.ToListAsync());

app.MapGet("/animals/{id}", async (int id, AnimalContext db) =>
    await db.Animals.FindAsync(id) is Animal animal ? Results.Ok(animal) : Results.NotFound("Animal not found"));

app.MapPost("/animals", async (Animal animal, AnimalContext db) =>
{
    db.Animals.Add(animal);
    await db.SaveChangesAsync();
    return Results.Created($"/animals/{animal.Id}", animal);
});

app.MapPut("/animals/{id}", async (int id, Animal animal, AnimalContext db) =>
{
    var existingAnimal = await db.Animals.FindAsync(id);
    if (existingAnimal is null) return Results.NotFound("Animal not found");

    existingAnimal.Nome = animal.Nome;
    existingAnimal.Idade = animal.Idade;
    existingAnimal.Cor = animal.Cor;
    existingAnimal.Tipo = animal.Tipo;
    existingAnimal.Peso = animal.Peso;

    await db.SaveChangesAsync();
    return Results.Ok(existingAnimal);
});

app.MapDelete("/animals/{id}", async (int id, AnimalContext db) =>
{
    var animal = await db.Animals.FindAsync(id);
    if (animal is null) return Results.NotFound("Animal not found");

    db.Animals.Remove(animal);
    await db.SaveChangesAsync();
    return Results.Ok("Animal deleted");
});

app.Run();

class Animal
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public int Idade { get; set; }
    public string Cor { get; set; } = string.Empty;
    public string Tipo { get; set; } = string.Empty;
    public double Peso { get; set; }
}

class AnimalContext : DbContext
{
    public AnimalContext(DbContextOptions<AnimalContext> options) : base(options) { }

    public DbSet<Animal> Animals => Set<Animal>();
}