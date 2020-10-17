namespace CalculationEngineServ.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddValueToCommandedGenerator : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.CommandedGenerators", "CommandingValue", c => c.Single(nullable: false));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.CommandedGenerators", "CommandingValue", c => c.Single());
        }
    }
}
