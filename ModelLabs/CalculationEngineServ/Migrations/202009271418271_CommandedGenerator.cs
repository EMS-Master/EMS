namespace CalculationEngineServ.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class CommandedGenerator : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.CommandedGenerators",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Gid = c.Long(nullable: false),
                        CommandingFlag = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.CommandedGenerators");
        }
    }
}
