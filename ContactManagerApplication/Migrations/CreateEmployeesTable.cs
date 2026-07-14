using FluentMigrator;

namespace ContactManagerApplication.Migrations
{
    [Migration(1)]
    public class CreateEmployeesTable : Migration        
    {
        public override void Up()
        {
            Create.Table("Employees")
                .WithColumn("Id").AsGuid().PrimaryKey()
                .WithColumn("Name").AsString(100).NotNullable()
                .WithColumn("BirthDay").AsDate().Nullable()
                .WithColumn("IsMarried").AsBoolean().Nullable()
                .WithColumn("Phone").AsString(15).Nullable()
                .WithColumn("Salary").AsDecimal(18, 2).NotNullable();
        }

        public override void Down()
        {
            Delete.Table("Employees");
        }
    }
}
