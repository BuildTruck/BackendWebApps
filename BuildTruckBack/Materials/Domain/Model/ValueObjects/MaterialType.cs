using System;
using System.Collections.Generic;

namespace BuildTruckBack.Materials.Domain.Model.ValueObjects
{
    public record MaterialType
    {
        public string Value { get; init; }

        private static readonly HashSet<string> ValidTypes = new()
        {
            "CEMENTO",
            "ACERO",
            "PINTURA",
            "HERRAMIENTA",
            "LIMPIEZA",
            "OTRO"
        };

        public MaterialType(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Material type cannot be null or empty", nameof(value));

            var normalized = value.Trim().ToUpper();

            if (!ValidTypes.Contains(normalized))
                throw new ArgumentException($"Invalid material type: {value}. Valid types are: {string.Join(", ", ValidTypes)}", nameof(value));

            Value = normalized;
        }

        public static implicit operator string(MaterialType materialType) => materialType.Value;
        public static implicit operator MaterialType(string value) => new(value);

        public override string ToString() =>
            Value switch
            {
                "CEMENTO" => "Cemento",
                "ACERO" => "Acero",
                "PINTURA" => "Pintura",
                "HERRAMIENTA" => "Herramienta",
                "LIMPIEZA" => "Limpieza",
                "OTRO" => "Otro",
                _ => Value
            };

        // Tipos comunes predefinidos
        public static MaterialType Cement => new("CEMENTO");
        public static MaterialType Steel => new("ACERO");
        public static MaterialType Paint => new("PINTURA");
        public static MaterialType Tool => new("HERRAMIENTA");
        public static MaterialType Cleaning => new("LIMPIEZA");
        public static MaterialType Other => new("OTRO");
    }
}