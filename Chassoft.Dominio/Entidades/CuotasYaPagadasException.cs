using System;
namespace Chassoft.Dominio.Entidades
{
    public sealed class CuotasYaPagadasException : InvalidOperationException
    {
        public CuotasYaPagadasException(string mensaje, Exception inner) : base(mensaje, inner) { }
    }
}
