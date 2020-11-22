namespace CalculationEngineServ.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class DiscreteCounterNameMigration : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.DiscreteCounters", "Name", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.DiscreteCounters", "Name");
        }
    }
}
