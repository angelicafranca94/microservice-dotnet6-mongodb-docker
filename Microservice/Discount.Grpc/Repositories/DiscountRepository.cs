﻿using Dapper;
using Discount.Grpc.Entities;
using Npgsql;

namespace Discount.Grpc.Repositories;

public class DiscountRepository : IDiscountRepository
{
    private readonly IConfiguration _configuration;

    public DiscountRepository(IConfiguration configuration)
    {
        _configuration = configuration ?? throw new ArgumentException(nameof(configuration));
    }

    private NpgsqlConnection GetConnectionPostgreSQL()
    {
        return new NpgsqlConnection
            (_configuration.GetValue<string>("DatabaseSettings:ConnectionString"));
    }

    public async Task<Coupon> GetDiscount(string productName)
    {
        var connection = GetConnectionPostgreSQL();

        var coupon = await connection.QueryFirstOrDefaultAsync<Coupon>
            ("SELECT *FROM Coupon WHERE ProductName = @ProductName",
            new { ProductName = productName });

        if (coupon is null)
            return new Coupon
            {
                ProductName = "No Discount",
                Amount = 0,
                Description = "No Discount Desc"
            };

        return coupon;
    }

    public async Task<bool> CreateDiscount(Coupon coupon)
    {
        var connection = GetConnectionPostgreSQL();

        var affected = await connection.ExecuteAsync
            ("INSERT INTO Coupon (ProductName, Description, Amount)" +
            "VALUES (@ProductName, @Description, @Amount)",
            new
            {
                ProductName = coupon.ProductName,
                Description = coupon.Description,
                Amount = coupon.Amount
            });

        if (affected is 0)
            return false;

        return true;
    }

    public async Task<bool> UpdateDiscount(Coupon coupon)
    {
        var connection = GetConnectionPostgreSQL();

        var affected = await connection.ExecuteAsync
            ("UPDATE Coupon SET ProductName=@ProductName, Description=@Description," +
            " Amount=@Amount WHERE Id=@Id",
            new
            {
                ProductName = coupon.ProductName,
                Description = coupon.Description,
                Amount = coupon.Amount,
                Id = coupon.Id
            });

        if (affected is 0)
            return false;

        return true;
    }
    public async Task<bool> DeleteDiscount(string productName)
    {
        var connection = GetConnectionPostgreSQL();

        var affected = await connection.ExecuteAsync
            ("DELETE FROM Coupon WHERE ProductName=@ProductName",
            new { ProductName = productName });

        if (affected is 0)
            return false;

        return true;
    }
}
