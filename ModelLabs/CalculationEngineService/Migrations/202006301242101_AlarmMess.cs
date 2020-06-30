namespace CalculationEngineService.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AlarmMess : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Alarms", "AlarmMessage", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Alarms", "AlarmMessage");
        }
    }
}
