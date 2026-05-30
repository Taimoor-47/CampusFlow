using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CampusFlow.Migrations
{
    /// <inheritdoc />
    public partial class RoleBasedCleanup : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EndTime",
                table: "Assignments");

            migrationBuilder.DropColumn(
                name: "StartTime",
                table: "Assignments");

            migrationBuilder.RenameColumn(
                name: "password",
                table: "Students",
                newName: "Password");

            migrationBuilder.RenameColumn(
                name: "isActice",
                table: "Students",
                newName: "IsActive");

            migrationBuilder.RenameColumn(
                name: "Room",
                table: "Assignments",
                newName: "Title");

            migrationBuilder.RenameColumn(
                name: "CourseTitle",
                table: "Assignments",
                newName: "Description");

            migrationBuilder.AddColumn<DateTime>(
                name: "DueDate",
                table: "Assignments",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DueDate",
                table: "Assignments");

            migrationBuilder.RenameColumn(
                name: "Password",
                table: "Students",
                newName: "password");

            migrationBuilder.RenameColumn(
                name: "IsActive",
                table: "Students",
                newName: "isActice");

            migrationBuilder.RenameColumn(
                name: "Title",
                table: "Assignments",
                newName: "Room");

            migrationBuilder.RenameColumn(
                name: "Description",
                table: "Assignments",
                newName: "CourseTitle");

            migrationBuilder.AddColumn<TimeSpan>(
                name: "EndTime",
                table: "Assignments",
                type: "time",
                nullable: false,
                defaultValue: new TimeSpan(0, 0, 0, 0, 0));

            migrationBuilder.AddColumn<TimeSpan>(
                name: "StartTime",
                table: "Assignments",
                type: "time",
                nullable: false,
                defaultValue: new TimeSpan(0, 0, 0, 0, 0));
        }
    }
}
