using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PadTime.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "public");

            migrationBuilder.CreateTable(
                name: "matches",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    site_id = table.Column<Guid>(type: "uuid", nullable: false),
                    court_id = table.Column<Guid>(type: "uuid", nullable: false),
                    organizer_id = table.Column<Guid>(type: "uuid", nullable: false),
                    start_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    end_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    version = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_matches", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "members",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    subject = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    matricule = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    site_id = table.Column<Guid>(type: "uuid", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    version = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_members", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "organizer_debts",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    member_id = table.Column<Guid>(type: "uuid", nullable: false),
                    amount_cents = table.Column<int>(type: "integer", nullable: false),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    version = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_organizer_debts", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "payments",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    match_id = table.Column<Guid>(type: "uuid", nullable: false),
                    member_id = table.Column<Guid>(type: "uuid", nullable: false),
                    participant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    amount_cents = table.Column<int>(type: "integer", nullable: false),
                    purpose = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    state = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    idempotency_key = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    processed_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    version = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_payments", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "sites",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    street_number = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: false),
                    street = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    postcode = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    city = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    country = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    timezone = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    version = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_sites", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "participants",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    match_id = table.Column<Guid>(type: "uuid", nullable: false),
                    member_id = table.Column<Guid>(type: "uuid", nullable: false),
                    role = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    payment_status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    joined_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    paid_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_participants", x => x.id);
                    table.ForeignKey(
                        name: "FK_participants_matches_match_id",
                        column: x => x.match_id,
                        principalSchema: "public",
                        principalTable: "matches",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "courts",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    site_id = table.Column<Guid>(type: "uuid", nullable: false),
                    label = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_courts", x => x.id);
                    table.ForeignKey(
                        name: "FK_courts_sites_site_id",
                        column: x => x.site_id,
                        principalSchema: "public",
                        principalTable: "sites",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "site_closures",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    site_id = table.Column<Guid>(type: "uuid", nullable: false),
                    type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    reason = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    start_date = table.Column<DateOnly>(type: "date", nullable: false),
                    end_date = table.Column<DateOnly>(type: "date", nullable: false),
                    modified_opening_time = table.Column<TimeOnly>(type: "time without time zone", nullable: true),
                    modified_closing_time = table.Column<TimeOnly>(type: "time without time zone", nullable: true),
                    affected_court_ids = table.Column<Guid[]>(type: "uuid[]", nullable: true),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_site_closures", x => x.id);
                    table.ForeignKey(
                        name: "FK_site_closures_sites_site_id",
                        column: x => x.site_id,
                        principalSchema: "public",
                        principalTable: "sites",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "site_schedules",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    site_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    valid_from = table.Column<DateOnly>(type: "date", nullable: false),
                    valid_until = table.Column<DateOnly>(type: "date", nullable: true),
                    opening_time = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    closing_time = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    applicable_days = table.Column<int[]>(type: "integer[]", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    priority = table.Column<int>(type: "integer", nullable: false),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_site_schedules", x => x.id);
                    table.ForeignKey(
                        name: "FK_site_schedules_sites_site_id",
                        column: x => x.site_id,
                        principalSchema: "public",
                        principalTable: "sites",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_courts_created_at_utc",
                schema: "public",
                table: "courts",
                column: "created_at_utc");

            migrationBuilder.CreateIndex(
                name: "IX_courts_is_active",
                schema: "public",
                table: "courts",
                column: "is_active");

            migrationBuilder.CreateIndex(
                name: "IX_courts_site_active",
                schema: "public",
                table: "courts",
                columns: new[] { "site_id", "is_active" });

            migrationBuilder.CreateIndex(
                name: "IX_courts_site_id",
                schema: "public",
                table: "courts",
                column: "site_id");

            migrationBuilder.CreateIndex(
                name: "IX_courts_site_id_label_unique",
                schema: "public",
                table: "courts",
                columns: new[] { "site_id", "label" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_courts_site_label_active",
                schema: "public",
                table: "courts",
                columns: new[] { "site_id", "label", "is_active" });

            migrationBuilder.CreateIndex(
                name: "IX_matches_court_id_start_at_utc",
                schema: "public",
                table: "matches",
                columns: new[] { "court_id", "start_at_utc" },
                unique: true,
                filter: "status <> 'Cancelled'");

            migrationBuilder.CreateIndex(
                name: "IX_matches_organizer_id",
                schema: "public",
                table: "matches",
                column: "organizer_id");

            migrationBuilder.CreateIndex(
                name: "IX_matches_site_id",
                schema: "public",
                table: "matches",
                column: "site_id");

            migrationBuilder.CreateIndex(
                name: "IX_matches_start_at_utc",
                schema: "public",
                table: "matches",
                column: "start_at_utc");

            migrationBuilder.CreateIndex(
                name: "IX_matches_status",
                schema: "public",
                table: "matches",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "IX_members_matricule",
                schema: "public",
                table: "members",
                column: "matricule",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_members_subject",
                schema: "public",
                table: "members",
                column: "subject",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_organizer_debts_member_id",
                schema: "public",
                table: "organizer_debts",
                column: "member_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_participants_match_id_member_id",
                schema: "public",
                table: "participants",
                columns: new[] { "match_id", "member_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_participants_member_id",
                schema: "public",
                table: "participants",
                column: "member_id");

            migrationBuilder.CreateIndex(
                name: "IX_payments_idempotency_key",
                schema: "public",
                table: "payments",
                column: "idempotency_key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_payments_match_id",
                schema: "public",
                table: "payments",
                column: "match_id");

            migrationBuilder.CreateIndex(
                name: "IX_payments_member_id",
                schema: "public",
                table: "payments",
                column: "member_id");

            migrationBuilder.CreateIndex(
                name: "IX_site_closures_affected_courts",
                schema: "public",
                table: "site_closures",
                column: "affected_court_ids")
                .Annotation("Npgsql:IndexMethod", "gin");

            migrationBuilder.CreateIndex(
                name: "IX_site_closures_created_at_utc",
                schema: "public",
                table: "site_closures",
                column: "created_at_utc");

            migrationBuilder.CreateIndex(
                name: "IX_site_closures_end_date",
                schema: "public",
                table: "site_closures",
                column: "end_date");

            migrationBuilder.CreateIndex(
                name: "IX_site_closures_reason",
                schema: "public",
                table: "site_closures",
                column: "reason");

            migrationBuilder.CreateIndex(
                name: "IX_site_closures_site_id",
                schema: "public",
                table: "site_closures",
                column: "site_id");

            migrationBuilder.CreateIndex(
                name: "IX_site_closures_site_period",
                schema: "public",
                table: "site_closures",
                columns: new[] { "site_id", "start_date", "end_date" });

            migrationBuilder.CreateIndex(
                name: "IX_site_closures_site_type_period",
                schema: "public",
                table: "site_closures",
                columns: new[] { "site_id", "type", "start_date", "end_date" });

            migrationBuilder.CreateIndex(
                name: "IX_site_closures_start_date",
                schema: "public",
                table: "site_closures",
                column: "start_date");

            migrationBuilder.CreateIndex(
                name: "IX_site_closures_type",
                schema: "public",
                table: "site_closures",
                column: "type");

            migrationBuilder.CreateIndex(
                name: "IX_site_closures_updated_at_utc",
                schema: "public",
                table: "site_closures",
                column: "updated_at_utc");

            migrationBuilder.CreateIndex(
                name: "IX_site_schedules_created_at_utc",
                schema: "public",
                table: "site_schedules",
                column: "created_at_utc");

            migrationBuilder.CreateIndex(
                name: "IX_site_schedules_is_active",
                schema: "public",
                table: "site_schedules",
                column: "is_active");

            migrationBuilder.CreateIndex(
                name: "IX_site_schedules_overlap_detection",
                schema: "public",
                table: "site_schedules",
                columns: new[] { "site_id", "valid_from", "valid_until", "is_active" });

            migrationBuilder.CreateIndex(
                name: "IX_site_schedules_site_id",
                schema: "public",
                table: "site_schedules",
                column: "site_id");

            migrationBuilder.CreateIndex(
                name: "IX_site_schedules_site_priority_active",
                schema: "public",
                table: "site_schedules",
                columns: new[] { "site_id", "priority", "is_active" });

            migrationBuilder.CreateIndex(
                name: "IX_site_schedules_site_valid_priority",
                schema: "public",
                table: "site_schedules",
                columns: new[] { "site_id", "valid_from", "priority" });

            migrationBuilder.CreateIndex(
                name: "IX_site_schedules_updated_at_utc",
                schema: "public",
                table: "site_schedules",
                column: "updated_at_utc");

            migrationBuilder.CreateIndex(
                name: "IX_site_schedules_validity_period",
                schema: "public",
                table: "site_schedules",
                columns: new[] { "valid_from", "valid_until" });

            migrationBuilder.CreateIndex(
                name: "IX_sites_active_location",
                schema: "public",
                table: "sites",
                columns: new[] { "is_active", "city", "country" });

            migrationBuilder.CreateIndex(
                name: "IX_sites_city_search",
                schema: "public",
                table: "sites",
                column: "city");

            migrationBuilder.CreateIndex(
                name: "IX_sites_country_search",
                schema: "public",
                table: "sites",
                column: "country");

            migrationBuilder.CreateIndex(
                name: "IX_sites_created_at_utc",
                schema: "public",
                table: "sites",
                column: "created_at_utc");

            migrationBuilder.CreateIndex(
                name: "IX_sites_created_id_pagination",
                schema: "public",
                table: "sites",
                columns: new[] { "created_at_utc", "id" });

            migrationBuilder.CreateIndex(
                name: "IX_sites_is_active_filter",
                schema: "public",
                table: "sites",
                column: "is_active");

            migrationBuilder.CreateIndex(
                name: "IX_sites_name_city_unique",
                schema: "public",
                table: "sites",
                columns: new[] { "name", "city" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_sites_name_search",
                schema: "public",
                table: "sites",
                column: "name");

            migrationBuilder.CreateIndex(
                name: "IX_sites_updated_at_utc",
                schema: "public",
                table: "sites",
                column: "updated_at_utc");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "courts",
                schema: "public");

            migrationBuilder.DropTable(
                name: "members",
                schema: "public");

            migrationBuilder.DropTable(
                name: "organizer_debts",
                schema: "public");

            migrationBuilder.DropTable(
                name: "participants",
                schema: "public");

            migrationBuilder.DropTable(
                name: "payments",
                schema: "public");

            migrationBuilder.DropTable(
                name: "site_closures",
                schema: "public");

            migrationBuilder.DropTable(
                name: "site_schedules",
                schema: "public");

            migrationBuilder.DropTable(
                name: "matches",
                schema: "public");

            migrationBuilder.DropTable(
                name: "sites",
                schema: "public");
        }
    }
}
