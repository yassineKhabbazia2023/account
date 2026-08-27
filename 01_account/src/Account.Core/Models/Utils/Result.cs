// <copyright file="Result.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

namespace Pulse.Account.Core.Models.Utils;

public enum ResultStatus
{
    Success,
    NotFound
}

public sealed class Result<T>
{
    private Result(ResultStatus status, T? value)
    {
        Status = status;
        Value = value;
    }

    public ResultStatus Status { get; }

    public T? Value { get; }

    public bool IsSuccess => Status == ResultStatus.Success;

    public static Result<T> Success(T value)
    {
        return new Result<T>(ResultStatus.Success, value);
    }

    public static Result<T> NotFound()
    {
        return new Result<T>(ResultStatus.NotFound, default);
    }
}

public sealed class Result
{
    private Result(ResultStatus status)
    {
        Status = status;
    }

    public ResultStatus Status { get; }

    public bool IsSuccess => Status == ResultStatus.Success;

    public static Result Success()
    {
        return new Result(ResultStatus.Success);
    }

    public static Result NotFound()
    {
        return new Result(ResultStatus.NotFound);
    }
}
