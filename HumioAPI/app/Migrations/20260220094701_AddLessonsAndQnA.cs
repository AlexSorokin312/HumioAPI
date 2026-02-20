using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace HumioAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddLessonsAndQnA : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "lessons",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "text", nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    module_id = table.Column<long>(type: "bigint", nullable: false),
                    order_index = table.Column<int>(type: "integer", nullable: false),
                    duration_seconds = table.Column<int>(type: "integer", nullable: false),
                    is_published = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    is_free = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_lessons", x => x.id);
                    table.CheckConstraint("ck_lessons_duration_seconds_positive", "duration_seconds > 0");
                    table.CheckConstraint("ck_lessons_order_index_non_negative", "order_index >= 0");
                    table.ForeignKey(
                        name: "fk_lessons_modules_module_id",
                        column: x => x.module_id,
                        principalTable: "modules",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "lessons_localizations",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    lesson_id = table.Column<long>(type: "bigint", nullable: false),
                    title = table.Column<string>(type: "text", nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    text_content = table.Column<string>(type: "text", nullable: true),
                    audio_link = table.Column<string>(type: "text", nullable: true),
                    language_code = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_lessons_localizations", x => x.id);
                    table.ForeignKey(
                        name: "fk_lessons_localizations_lessons_lesson_id",
                        column: x => x.lesson_id,
                        principalTable: "lessons",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "questions",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "text", nullable: false),
                    content = table.Column<string>(type: "text", nullable: false),
                    lesson_id = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_questions", x => x.id);
                    table.ForeignKey(
                        name: "fk_questions_lessons_lesson_id",
                        column: x => x.lesson_id,
                        principalTable: "lessons",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "lessons_links",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    localization_id = table.Column<long>(type: "bigint", nullable: false),
                    pos = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_lessons_links", x => x.id);
                    table.ForeignKey(
                        name: "fk_lessons_links_lessons_localizations_localization_id",
                        column: x => x.localization_id,
                        principalTable: "lessons_localizations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "answers",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    question_id = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_answers", x => x.id);
                    table.ForeignKey(
                        name: "fk_answers_questions_question_id",
                        column: x => x.question_id,
                        principalTable: "questions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "question_localizations",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    question_id = table.Column<long>(type: "bigint", nullable: false),
                    language_code = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    question_text = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_question_localizations", x => x.id);
                    table.ForeignKey(
                        name: "fk_question_localizations_questions_question_id",
                        column: x => x.question_id,
                        principalTable: "questions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "answer_localizations",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    answer_id = table.Column<long>(type: "bigint", nullable: false),
                    language_code = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    answer_text = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_answer_localizations", x => x.id);
                    table.ForeignKey(
                        name: "fk_answer_localizations_answers_answer_id",
                        column: x => x.answer_id,
                        principalTable: "answers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_answer_localizations_answer_id",
                table: "answer_localizations",
                column: "answer_id");

            migrationBuilder.CreateIndex(
                name: "ix_answer_localizations_answer_id_language_code",
                table: "answer_localizations",
                columns: new[] { "answer_id", "language_code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_answers_question_id",
                table: "answers",
                column: "question_id");

            migrationBuilder.CreateIndex(
                name: "ix_lessons_module_id",
                table: "lessons",
                column: "module_id");

            migrationBuilder.CreateIndex(
                name: "ix_lessons_module_id_order_index",
                table: "lessons",
                columns: new[] { "module_id", "order_index" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_lessons_links_localization_id",
                table: "lessons_links",
                column: "localization_id");

            migrationBuilder.CreateIndex(
                name: "ix_lessons_links_localization_id_pos",
                table: "lessons_links",
                columns: new[] { "localization_id", "pos" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_lessons_localizations_lesson_id",
                table: "lessons_localizations",
                column: "lesson_id");

            migrationBuilder.CreateIndex(
                name: "ix_lessons_localizations_lesson_id_language_code",
                table: "lessons_localizations",
                columns: new[] { "lesson_id", "language_code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_question_localizations_question_id",
                table: "question_localizations",
                column: "question_id");

            migrationBuilder.CreateIndex(
                name: "ix_question_localizations_question_id_language_code",
                table: "question_localizations",
                columns: new[] { "question_id", "language_code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_questions_lesson_id",
                table: "questions",
                column: "lesson_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "answer_localizations");

            migrationBuilder.DropTable(
                name: "lessons_links");

            migrationBuilder.DropTable(
                name: "question_localizations");

            migrationBuilder.DropTable(
                name: "answers");

            migrationBuilder.DropTable(
                name: "lessons_localizations");

            migrationBuilder.DropTable(
                name: "questions");

            migrationBuilder.DropTable(
                name: "lessons");
        }
    }
}
