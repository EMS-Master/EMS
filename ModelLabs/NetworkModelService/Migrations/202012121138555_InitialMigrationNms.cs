namespace FTN.Services.NetworkModelService.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class InitialMigrationNms : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.DeltaModels",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Time = c.DateTime(nullable: false),
                        Delta = c.Binary(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.DeltaModels");
        }
    }
}
