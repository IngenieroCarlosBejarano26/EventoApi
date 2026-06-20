using EventosVivos.Domain.Common;

namespace EventosVivos.Domain.Entities;

/// <summary>
/// Lugar físico donde se realizan los eventos. Es dato de referencia (seed) con capacidad fija.
/// </summary>
public sealed class Venue : Entity
{
    public int Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public int Capacity { get; private set; }
    public string City { get; private set; } = string.Empty;

    private Venue() { } // EF Core

    public Venue(int id, string name, int capacity, string city)
    {
        Id = id;
        Name = name;
        Capacity = capacity;
        City = city;
    }
}
