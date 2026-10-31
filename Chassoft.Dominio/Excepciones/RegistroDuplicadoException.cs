using System;
namespace Chassoft.Dominio.Excepciones
{
    public sealed class RegistroDuplicadoException : ArgumentException
    {
        public RegistroDuplicadoException(string mensaje) : base(mensaje) { }
    }
}
