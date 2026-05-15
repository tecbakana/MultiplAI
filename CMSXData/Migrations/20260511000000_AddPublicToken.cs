using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CMSXData.Migrations
{
    // IF NOT EXISTS: InitialCreate já declara publictoken para installs do zero.
    // Esta migration existe para databases PostgreSQL que já tinham as demais tabelas
    // mas foram criadas antes de publictoken ser introduzido no modelo.
    public partial class AddPublicToken : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // SQL Server: InitialCreate já cria publictoken — esta migration é no-op.
            if (migrationBuilder.ActiveProvider == "Microsoft.EntityFrameworkCore.SqlServer")
                return;

            migrationBuilder.Sql("""
                CREATE TABLE IF NOT EXISTS publictoken (
                    publictokenid   uuid                     NOT NULL DEFAULT gen_random_uuid(),
                    token           character varying(100)   NOT NULL,
                    aplicacaoid     character varying(64)    NOT NULL,
                    ativo           boolean                  NOT NULL,
                    datainclusao    timestamp with time zone NOT NULL DEFAULT CURRENT_TIMESTAMP,
                    datavencimento  timestamp with time zone,
                    CONSTRAINT "PK_publictoken" PRIMARY KEY (publictokenid)
                );

                CREATE INDEX IF NOT EXISTS "IX_publictoken_aplicacaoid"
                    ON publictoken (aplicacaoid);

                CREATE UNIQUE INDEX IF NOT EXISTS "UQ_publictoken_token"
                    ON publictoken (token);
                """);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            if (migrationBuilder.ActiveProvider == "Microsoft.EntityFrameworkCore.SqlServer")
                return;

            migrationBuilder.Sql("""
                DROP INDEX IF EXISTS "UQ_publictoken_token";
                DROP INDEX IF EXISTS "IX_publictoken_aplicacaoid";
                DROP TABLE IF EXISTS publictoken;
                """);
        }
    }
}
