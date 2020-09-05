namespace CalculationEngineServ.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class EmissionCO2 : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.CO2Emission",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        PreviousEmission = c.Single(nullable: false),
                        CurrentEmission = c.Single(nullable: false),
                        Timestamp = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            AddColumn("dbo.TotalProductions", "TotalCost", c => c.Single(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.TotalProductions", "TotalCost");
            DropTable("dbo.CO2Emission");
        }
    }
}
