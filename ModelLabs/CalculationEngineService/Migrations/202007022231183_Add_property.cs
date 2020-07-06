namespace CalculationEngineService.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Add_property : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Alarms", "AlarmTimeStamp", c => c.DateTime(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Alarms", "AlarmTimeStamp");
        }
    }
}
