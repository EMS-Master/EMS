namespace CalculationEngineServ.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AlarmNameMigration : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Alarms", "Name", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Alarms", "Name");
        }
    }
}
