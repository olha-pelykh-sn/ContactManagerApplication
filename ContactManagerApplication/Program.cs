using ContactManagerApplication.Data;
using ContactManagerApplication.Migrations;
using ContactManagerApplication.Repositories;
using ContactManagerApplication.Services;
using ContactManagerApplication.Mappings;
using FluentMigrator.Runner;
using Dapper;

namespace ContactManagerApplication
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllersWithViews();

            // Register Dapper type handlers
            SqlMapper.AddTypeHandler(new DateOnlyTypeHandler());

            //Register Dapper DbContext as Singleton
            builder.Services.AddSingleton<ContactManagerDbContext>();

            //Register Repository as Scoped
            builder.Services.AddScoped<IEmployeeRepository, EmployeeRepository>();

            //Register CSV import service as Scoped
            builder.Services.AddScoped<ICsvImportService, CsvImportService>();

            //Add AutoMapper
            builder.Services.AddAutoMapper(cfg => cfg.AddMaps(typeof(EmployeeProfile).Assembly));

            //Add FluentMigrator to services
            var connectionString = builder.Configuration.GetConnectionString("ContactManagerDbConnectionString");

            builder.Services.AddFluentMigratorCore()
                .ConfigureRunner(rb => rb
                    .AddSqlServer()
                    .WithGlobalConnectionString(connectionString)
                    .ScanIn(typeof(CreateEmployeesTable).Assembly).For.Migrations())
                .AddLogging(lb => lb.AddFluentMigratorConsole());

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseRouting();

            app.UseAuthorization();

            app.MapStaticAssets();
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Employee}/{action=GetAllEmployees}/{id?}")
                .WithStaticAssets();
            
            //Migration executing
            using (var scope = app.Services.CreateScope())
            {
                var runner = scope.ServiceProvider.GetRequiredService<IMigrationRunner>();
                runner.MigrateUp();
            }

            app.Run();
        }
    }
}
