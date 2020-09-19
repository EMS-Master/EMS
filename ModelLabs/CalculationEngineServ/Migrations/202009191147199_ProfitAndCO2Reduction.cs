namespace CalculationEngineServ.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ProfitAndCO2Reduction : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.TotalProductions", "CO2Reduction", c => c.Single(nullable: false));
            AddColumn("dbo.TotalProductions", "CO2Emission", c => c.Single(nullable: false));
            AddColumn("dbo.TotalProductions", "Profit", c => c.Single(nullable: false));
            DropTable("dbo.CO2Emission");
        }
        
        public override void Down()
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
            
            DropColumn("dbo.TotalProductions", "Profit");
            DropColumn("dbo.TotalProductions", "CO2Emission");
            DropColumn("dbo.TotalProductions", "CO2Reduction");
        }
    }
}
