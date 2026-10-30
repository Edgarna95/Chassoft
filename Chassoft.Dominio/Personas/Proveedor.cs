using System;

namespace Chassoft.Dominio.Personas
{
    public class Proveedor
    {
        public int Id { get; private set; }
        public string Nombre { get; private set; }
        public string Cuit { get; private set; }
        public string? RazonSocial { get; private set; }
        public string? TelefonoDeContacto { get; private set; }
        public string? CorreoDeContacto { get; private set; }
        public string? CondicionIVA { get; private set; }
        public string? CbuAsociado { get; private set; }

        public Proveedor(string nombre, string cuit)
            : this(nombre, cuit, null, null, null, null, null)
        {
        }

        public Proveedor(string nombre, string cuit, string? razonSocial,
            string? telefonoDeContacto, string? correoDeContacto,
            string? condicionIVA, string? cbuAsociado)
        {
            Nombre = ValidarObligatorio(nombre, 100, nameof(nombre), "El nombre es obligatorio");
            Cuit = ValidarObligatorio(cuit, 20, nameof(cuit), "El CUIT es obligatorio");
            RazonSocial = NormalizarOpcional(razonSocial, 200, nameof(razonSocial));
            TelefonoDeContacto = NormalizarOpcional(telefonoDeContacto, 50, nameof(telefonoDeContacto));
            CorreoDeContacto = NormalizarOpcional(correoDeContacto, 254, nameof(correoDeContacto));
            CondicionIVA = NormalizarOpcional(condicionIVA, 50, nameof(condicionIVA));
            CbuAsociado = NormalizarOpcional(cbuAsociado, 22, nameof(cbuAsociado));
        }

        public Proveedor(int id, string nombre, string cuit)
            : this(id, nombre, cuit, null, null, null, null, null)
        {
        }

        public Proveedor(int id, string nombre, string cuit, string? razonSocial,
            string? telefonoDeContacto, string? correoDeContacto,
            string? condicionIVA, string? cbuAsociado)
            : this(nombre, cuit, razonSocial, telefonoDeContacto, correoDeContacto,
                  condicionIVA, cbuAsociado)
        {
            if (id <= 0)
                throw new ArgumentOutOfRangeException(nameof(id), "El identificador debe ser mayor a cero");
            Id = id;
        }

        private static string ValidarObligatorio(string valor, int longitudMaxima,
            string parametro, string mensaje)
        {
            return NormalizarOpcional(valor, longitudMaxima, parametro)
                ?? throw new ArgumentException(mensaje, parametro);
        }

        private static string? NormalizarOpcional(string? valor, int longitudMaxima, string parametro)
        {
            if (valor == null || string.IsNullOrWhiteSpace(valor)) return null;
            var normalizado = valor.Trim();
            if (normalizado.Length > longitudMaxima)
                throw new ArgumentException(
                    $"El campo {parametro} admite hasta {longitudMaxima} caracteres", parametro);
            return normalizado;
        }
    }
}
