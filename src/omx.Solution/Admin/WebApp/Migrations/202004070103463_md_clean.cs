namespace WebApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class md_clean : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.Departments", "CompanyId", "dbo.Companies");
            DropForeignKey("dbo.Employees", "CompanyId", "dbo.Companies");
            DropForeignKey("dbo.Employees", "DepartmentId", "dbo.Departments");
            DropIndex("dbo.Departments", new[] { "CompanyId" });
            DropIndex("dbo.Employees", new[] { "Name" });
            DropIndex("dbo.Employees", new[] { "CompanyId" });
            DropIndex("dbo.Employees", new[] { "DepartmentId" });
            DropTable("dbo.Departments");
            DropTable("dbo.Employees");
        }
        
        public override void Down()
        {
            CreateTable(
                "dbo.Employees",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, maxLength: 20),
                        Title = c.String(maxLength: 30),
                        PhoneNumber = c.String(maxLength: 30),
                        WX = c.String(maxLength: 30),
                        Sex = c.String(nullable: false, maxLength: 10),
                        Age = c.Int(nullable: false),
                        Brithday = c.DateTime(nullable: false),
                        EntryDate = c.DateTime(nullable: false),
                        IsDeleted = c.Boolean(nullable: false),
                        LeaveDate = c.DateTime(),
                        CompanyId = c.Int(nullable: false),
                        DepartmentId = c.Int(nullable: false),
                        CreatedDate = c.DateTime(),
                        CreatedBy = c.String(maxLength: 20),
                        LastModifiedDate = c.DateTime(),
                        LastModifiedBy = c.String(maxLength: 20),
                        TenantId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Departments",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(maxLength: 10),
                        Manager = c.String(maxLength: 10),
                        CompanyId = c.Int(nullable: false),
                        CreatedDate = c.DateTime(),
                        CreatedBy = c.String(maxLength: 20),
                        LastModifiedDate = c.DateTime(),
                        LastModifiedBy = c.String(maxLength: 20),
                        TenantId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateIndex("dbo.Employees", "DepartmentId");
            CreateIndex("dbo.Employees", "CompanyId");
            CreateIndex("dbo.Employees", "Name", unique: true);
            CreateIndex("dbo.Departments", "CompanyId");
            AddForeignKey("dbo.Employees", "DepartmentId", "dbo.Departments", "Id", cascadeDelete: true);
            AddForeignKey("dbo.Employees", "CompanyId", "dbo.Companies", "Id", cascadeDelete: true);
            AddForeignKey("dbo.Departments", "CompanyId", "dbo.Companies", "Id", cascadeDelete: true);
        }
    }
}
