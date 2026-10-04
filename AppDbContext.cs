using Microsoft.EntityFrameworkCore;
using QualityEvalAPI.Models;

namespace QualityEvalAPI.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Role> Roles => Set<Role>();
    public DbSet<User> Users => Set<User>();
    public DbSet<LoginLog> LoginLogs => Set<LoginLog>();

    public DbSet<CriteriaGroup> CriteriaGroups => Set<CriteriaGroup>();
    public DbSet<Criteria> Criteria => Set<Criteria>();

    public DbSet<Survey> Surveys => Set<Survey>();
    public DbSet<SurveyQuestion> SurveyQuestions => Set<SurveyQuestion>();
    public DbSet<SurveyResponse> SurveyResponses => Set<SurveyResponse>();
    public DbSet<SurveyAnswer> SurveyAnswers => Set<SurveyAnswer>();

    public DbSet<EvaluationPeriod> EvaluationPeriods => Set<EvaluationPeriod>();
    public DbSet<EvaluationTarget> EvaluationTargets => Set<EvaluationTarget>();
    public DbSet<EvaluationScore> EvaluationScores => Set<EvaluationScore>();
    public DbSet<AhpMatrix> AhpMatrices => Set<AhpMatrix>();
    public DbSet<QualityRanking> QualityRankings => Set<QualityRanking>();
    public DbSet<Report> Reports => Set<Report>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Role>().ToTable("roles").HasKey(r => r.RoleId);
        modelBuilder.Entity<Role>().Property(r => r.RoleId).HasColumnName("role_id");
        modelBuilder.Entity<Role>().Property(r => r.RoleCode).HasColumnName("role_code");
        modelBuilder.Entity<Role>().Property(r => r.RoleName).HasColumnName("role_name");
        modelBuilder.Entity<Role>().Property(r => r.Description).HasColumnName("description");

        modelBuilder.Entity<User>().ToTable("users").HasKey(u => u.UserId);
        modelBuilder.Entity<User>().Property(u => u.UserId).HasColumnName("user_id");
        modelBuilder.Entity<User>().Property(u => u.Username).HasColumnName("username");
        modelBuilder.Entity<User>().Property(u => u.PasswordHash).HasColumnName("password_hash");
        modelBuilder.Entity<User>().Property(u => u.FullName).HasColumnName("full_name");
        modelBuilder.Entity<User>().Property(u => u.Email).HasColumnName("email");
        modelBuilder.Entity<User>().Property(u => u.Phone).HasColumnName("phone");
        modelBuilder.Entity<User>().Property(u => u.RoleId).HasColumnName("role_id");
        modelBuilder.Entity<User>().Property(u => u.Department).HasColumnName("department");
        modelBuilder.Entity<User>().Property(u => u.StudentCode).HasColumnName("student_code");
        modelBuilder.Entity<User>().Property(u => u.StaffCode).HasColumnName("staff_code");
        modelBuilder.Entity<User>().Property(u => u.Status).HasColumnName("status");
        modelBuilder.Entity<User>().Property(u => u.CreatedAt).HasColumnName("created_at");
        modelBuilder.Entity<User>().Property(u => u.UpdatedAt).HasColumnName("updated_at");
        modelBuilder.Entity<User>().HasOne(u => u.Role).WithMany(r => r.Users).HasForeignKey(u => u.RoleId);

        modelBuilder.Entity<LoginLog>().ToTable("login_logs").HasKey(l => l.LogId);
        modelBuilder.Entity<LoginLog>().Property(l => l.LogId).HasColumnName("log_id");
        modelBuilder.Entity<LoginLog>().Property(l => l.UserId).HasColumnName("user_id");
        modelBuilder.Entity<LoginLog>().Property(l => l.Action).HasColumnName("action");
        modelBuilder.Entity<LoginLog>().Property(l => l.IpAddress).HasColumnName("ip_address");
        modelBuilder.Entity<LoginLog>().Property(l => l.CreatedAt).HasColumnName("created_at");

        modelBuilder.Entity<CriteriaGroup>().ToTable("criteria_groups").HasKey(g => g.GroupId);
        modelBuilder.Entity<CriteriaGroup>().Property(g => g.GroupId).HasColumnName("group_id");
        modelBuilder.Entity<CriteriaGroup>().Property(g => g.GroupCode).HasColumnName("group_code");
        modelBuilder.Entity<CriteriaGroup>().Property(g => g.GroupName).HasColumnName("group_name");
        modelBuilder.Entity<CriteriaGroup>().Property(g => g.Description).HasColumnName("description");
        modelBuilder.Entity<CriteriaGroup>().Property(g => g.CreatedAt).HasColumnName("created_at");

        modelBuilder.Entity<Criteria>().ToTable("criteria").HasKey(c => c.CriteriaId);
        modelBuilder.Entity<Criteria>().Property(c => c.CriteriaId).HasColumnName("criteria_id");
        modelBuilder.Entity<Criteria>().Property(c => c.GroupId).HasColumnName("group_id");
        modelBuilder.Entity<Criteria>().Property(c => c.CriteriaCode).HasColumnName("criteria_code");
        modelBuilder.Entity<Criteria>().Property(c => c.CriteriaName).HasColumnName("criteria_name");
        modelBuilder.Entity<Criteria>().Property(c => c.Description).HasColumnName("description");
        modelBuilder.Entity<Criteria>().Property(c => c.Weight).HasColumnName("weight");
        modelBuilder.Entity<Criteria>().Property(c => c.ScaleMin).HasColumnName("scale_min");
        modelBuilder.Entity<Criteria>().Property(c => c.ScaleMax).HasColumnName("scale_max");
        modelBuilder.Entity<Criteria>().Property(c => c.IsActive).HasColumnName("is_active");
        modelBuilder.Entity<Criteria>().Property(c => c.CreatedBy).HasColumnName("created_by");
        modelBuilder.Entity<Criteria>().Property(c => c.CreatedAt).HasColumnName("created_at");
        modelBuilder.Entity<Criteria>().Property(c => c.UpdatedAt).HasColumnName("updated_at");
        modelBuilder.Entity<Criteria>().HasOne(c => c.Group).WithMany(g => g.Criteria).HasForeignKey(c => c.GroupId);

        modelBuilder.Entity<Survey>().ToTable("surveys").HasKey(s => s.SurveyId);
        modelBuilder.Entity<Survey>().Property(s => s.SurveyId).HasColumnName("survey_id");
        modelBuilder.Entity<Survey>().Property(s => s.Title).HasColumnName("title");
        modelBuilder.Entity<Survey>().Property(s => s.Description).HasColumnName("description");
        modelBuilder.Entity<Survey>().Property(s => s.TargetRole).HasColumnName("target_role");
        modelBuilder.Entity<Survey>().Property(s => s.StartDate).HasColumnName("start_date");
        modelBuilder.Entity<Survey>().Property(s => s.EndDate).HasColumnName("end_date");
        modelBuilder.Entity<Survey>().Property(s => s.Status).HasColumnName("status");
        modelBuilder.Entity<Survey>().Property(s => s.CreatedBy).HasColumnName("created_by");
        modelBuilder.Entity<Survey>().Property(s => s.CreatedAt).HasColumnName("created_at");

        modelBuilder.Entity<SurveyQuestion>().ToTable("survey_questions").HasKey(q => q.QuestionId);
        modelBuilder.Entity<SurveyQuestion>().Property(q => q.QuestionId).HasColumnName("question_id");
        modelBuilder.Entity<SurveyQuestion>().Property(q => q.SurveyId).HasColumnName("survey_id");
        modelBuilder.Entity<SurveyQuestion>().Property(q => q.CriteriaId).HasColumnName("criteria_id");
        modelBuilder.Entity<SurveyQuestion>().Property(q => q.QuestionText).HasColumnName("question_text");
        modelBuilder.Entity<SurveyQuestion>().Property(q => q.QuestionType).HasColumnName("question_type");
        modelBuilder.Entity<SurveyQuestion>().Property(q => q.OptionsJson).HasColumnName("options_json").HasColumnType("json");
        modelBuilder.Entity<SurveyQuestion>().Property(q => q.OrderNo).HasColumnName("order_no");
        modelBuilder.Entity<SurveyQuestion>().HasOne<Survey>().WithMany(s => s.Questions).HasForeignKey(q => q.SurveyId);

        modelBuilder.Entity<SurveyResponse>().ToTable("survey_responses").HasKey(r => r.ResponseId);
        modelBuilder.Entity<SurveyResponse>().Property(r => r.ResponseId).HasColumnName("response_id");
        modelBuilder.Entity<SurveyResponse>().Property(r => r.SurveyId).HasColumnName("survey_id");
        modelBuilder.Entity<SurveyResponse>().Property(r => r.UserId).HasColumnName("user_id");
        modelBuilder.Entity<SurveyResponse>().Property(r => r.SubmittedAt).HasColumnName("submitted_at");

        modelBuilder.Entity<SurveyAnswer>().ToTable("survey_answers").HasKey(a => a.AnswerId);
        modelBuilder.Entity<SurveyAnswer>().Property(a => a.AnswerId).HasColumnName("answer_id");
        modelBuilder.Entity<SurveyAnswer>().Property(a => a.ResponseId).HasColumnName("response_id");
        modelBuilder.Entity<SurveyAnswer>().Property(a => a.QuestionId).HasColumnName("question_id");
        modelBuilder.Entity<SurveyAnswer>().Property(a => a.AnswerValue).HasColumnName("answer_value");
        modelBuilder.Entity<SurveyAnswer>().Property(a => a.AnswerText).HasColumnName("answer_text");
        modelBuilder.Entity<SurveyAnswer>().HasOne<SurveyResponse>().WithMany(r => r.Answers).HasForeignKey(a => a.ResponseId);

        modelBuilder.Entity<EvaluationPeriod>().ToTable("evaluation_periods").HasKey(p => p.PeriodId);
        modelBuilder.Entity<EvaluationPeriod>().Property(p => p.PeriodId).HasColumnName("period_id");
        modelBuilder.Entity<EvaluationPeriod>().Property(p => p.PeriodName).HasColumnName("period_name");
        modelBuilder.Entity<EvaluationPeriod>().Property(p => p.StartDate).HasColumnName("start_date");
        modelBuilder.Entity<EvaluationPeriod>().Property(p => p.EndDate).HasColumnName("end_date");
        modelBuilder.Entity<EvaluationPeriod>().Property(p => p.Status).HasColumnName("status");

        modelBuilder.Entity<EvaluationTarget>().ToTable("evaluation_targets").HasKey(t => t.TargetId);
        modelBuilder.Entity<EvaluationTarget>().Property(t => t.TargetId).HasColumnName("target_id");
        modelBuilder.Entity<EvaluationTarget>().Property(t => t.TargetType).HasColumnName("target_type");
        modelBuilder.Entity<EvaluationTarget>().Property(t => t.TargetName).HasColumnName("target_name");
        modelBuilder.Entity<EvaluationTarget>().Property(t => t.RefId).HasColumnName("ref_id");

        modelBuilder.Entity<EvaluationScore>().ToTable("evaluation_scores").HasKey(s => s.ScoreId);
        modelBuilder.Entity<EvaluationScore>().Property(s => s.ScoreId).HasColumnName("score_id");
        modelBuilder.Entity<EvaluationScore>().Property(s => s.PeriodId).HasColumnName("period_id");
        modelBuilder.Entity<EvaluationScore>().Property(s => s.TargetId).HasColumnName("target_id");
        modelBuilder.Entity<EvaluationScore>().Property(s => s.CriteriaId).HasColumnName("criteria_id");
        modelBuilder.Entity<EvaluationScore>().Property(s => s.RawScore).HasColumnName("raw_score");
        modelBuilder.Entity<EvaluationScore>().Property(s => s.WeightedScore).HasColumnName("weighted_score");
        modelBuilder.Entity<EvaluationScore>().Property(s => s.Method).HasColumnName("method");
        modelBuilder.Entity<EvaluationScore>().Property(s => s.ComputedAt).HasColumnName("computed_at");

        modelBuilder.Entity<AhpMatrix>().ToTable("ahp_matrices").HasKey(m => m.MatrixId);
        modelBuilder.Entity<AhpMatrix>().Property(m => m.MatrixId).HasColumnName("matrix_id");
        modelBuilder.Entity<AhpMatrix>().Property(m => m.GroupId).HasColumnName("group_id");
        modelBuilder.Entity<AhpMatrix>().Property(m => m.CriteriaAId).HasColumnName("criteria_a_id");
        modelBuilder.Entity<AhpMatrix>().Property(m => m.CriteriaBId).HasColumnName("criteria_b_id");
        modelBuilder.Entity<AhpMatrix>().Property(m => m.ComparisonValue).HasColumnName("comparison_value");
        modelBuilder.Entity<AhpMatrix>().Property(m => m.CreatedBy).HasColumnName("created_by");
        modelBuilder.Entity<AhpMatrix>().Property(m => m.CreatedAt).HasColumnName("created_at");

        modelBuilder.Entity<QualityRanking>().ToTable("quality_rankings").HasKey(r => r.RankingId);
        modelBuilder.Entity<QualityRanking>().Property(r => r.RankingId).HasColumnName("ranking_id");
        modelBuilder.Entity<QualityRanking>().Property(r => r.PeriodId).HasColumnName("period_id");
        modelBuilder.Entity<QualityRanking>().Property(r => r.TargetId).HasColumnName("target_id");
        modelBuilder.Entity<QualityRanking>().Property(r => r.TotalScore).HasColumnName("total_score");
        modelBuilder.Entity<QualityRanking>().Property(r => r.RankNo).HasColumnName("rank_no");
        modelBuilder.Entity<QualityRanking>().Property(r => r.RankLabel).HasColumnName("rank_label");
        modelBuilder.Entity<QualityRanking>().Property(r => r.CreatedAt).HasColumnName("created_at");

        modelBuilder.Entity<Report>().ToTable("reports").HasKey(r => r.ReportId);
        modelBuilder.Entity<Report>().Property(r => r.ReportId).HasColumnName("report_id");
        modelBuilder.Entity<Report>().Property(r => r.PeriodId).HasColumnName("period_id");
        modelBuilder.Entity<Report>().Property(r => r.Title).HasColumnName("title");
        modelBuilder.Entity<Report>().Property(r => r.FilePath).HasColumnName("file_path");
        modelBuilder.Entity<Report>().Property(r => r.GeneratedBy).HasColumnName("generated_by");
        modelBuilder.Entity<Report>().Property(r => r.GeneratedAt).HasColumnName("generated_at");
    }
}
