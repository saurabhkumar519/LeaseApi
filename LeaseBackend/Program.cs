using LeaseBackend.API.ActionFilter;
using LeaseBackend.API.Extension;
using LeaseBackend.API.SecurityHandler;
using LeaseBackend.BAL.Extension;
using LeaseBackend.DAL.Extension;
using LeaseBackend.Logger.ExceptionHandler;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers(option =>
{
    option.Filters.Add<SSOTokenValidateFilter>();

});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddApiProjectServices(builder.Configuration);
builder.Services.AddBusinessLayerServices(builder.Configuration);
builder.Services.AddDataAccessLayerServices(builder.Configuration);

var app = builder.Build();
app.ExceptionHandler();


if (app.Environment.IsDevelopment())
{
    app.UseSwaggerSecurity();
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseSwaggerSecurity();
    app.UseSwagger();
    app.UseSwaggerUI();
}


app.UseStaticFiles();
app.UseCors("AllowOrigin");
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
