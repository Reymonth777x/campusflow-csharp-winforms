using System;
using System.Collections.Generic;
using System.Text;

namespace CampusFlow.Application.Common;

public enum TipoError { Validacion, NoEncontrado, Conflicto }

public sealed record Error(TipoError Tipo, string Mensaje, string? Campo = null);

public class Result
{
    protected Result(IReadOnlyList<Error> errores) => Errores = errores;

    public IReadOnlyList<Error> Errores { get; }
    public bool EsExito => Errores.Count == 0;
    public bool EsFalla => !EsExito;

    public static Result Exito() => new([]);

    public static Result Falla(params Error[] errores)
    {
        ArgumentOutOfRangeException.ThrowIfZero(errores.Length);
        return new Result(errores);
    }
}

public sealed class Result<T> : Result
{
    private readonly T? _valor;

    private Result(T? valor, IReadOnlyList<Error> errores) : base(errores) => _valor = valor;

    public T Valor => EsExito
        ? _valor!
        : throw new InvalidOperationException("Un resultado fallido no tiene valor.");

    public static Result<T> Exito(T valor) => new(valor, []);

    public new static Result<T> Falla(params Error[] errores)
    {
        ArgumentOutOfRangeException.ThrowIfZero(errores.Length);
        return new Result<T>(default, errores);
    }
}
