namespace DeepMatch.Domain.Constants;

public static class BusinessRules
{
    public static class Ai
    {
        public const int MaxTagsPerUserInIcebreaker = 5;
        public const int MinTagsPerAnswer = 1;
        public const int MaxTagsPerAnswer = 3;
        public const int MinDevilsAdvocateSentences = 2;
        public const int MaxDevilsAdvocateSentences = 4;
    }

    public static class Answers
    {
        public const int MinTextLength = 10;
        public const int MaxTextLength = 5000;
        public const int AnalyzeTagsBatchSize = 10;
    }

    public static class Badges
    {
        public const int NameMaxLength = 100;
        public const int DescriptionMaxLength = 500;
        public const int TypeMaxLength = 50;
        public const int TaggedAnswersRequired = 10;
        public const int DistinctCategoriesRequired = 5;
        public const int LikesRequired = 20;
        public const int ConsecutiveAnswerDaysRequired = 7;
        public const int MatchesRequired = 5;
        public const int RatingReward = 10;
    }

    public static class Files
    {
        public const int MaxImageUploadSizeMegabytes = 5;
        public const int MaxImageUploadSizeBytes = MaxImageUploadSizeMegabytes * 1024 * 1024;
        public const int StoredFileNameMaxLength = 500;
    }

    public static class Messages
    {
        public const int MaxContentLength = 2000;
    }

    public static class Notifications
    {
        public const int TypeMaxLength = 50;
        public const int TitleMaxLength = 500;
        public const int LinkMaxLength = 200;
        public const int PageSize = 20;
    }

    public static class Questions
    {
        public const int MinTextLength = 10;
        public const int MaxTextLength = 2000;
        public const int CategoryMaxLength = 50;
    }

    public static class Rating
    {
        public const int DefaultIncreaseAmount = 1;
        public const int PointsPerBonusSwipe = 100;
        public const int AnswerReward = 5;
        public const int MatchReward = 20;
    }

    public static class Reports
    {
        public const int MinReasonLength = 5;
        public const int MaxReasonLength = 500;
        public const int AutoBlockReportsCount = 5;
    }

    public static class Swipes
    {
        public const int DirectionMaxLength = 10;
        public const int BaseDailyLimit = 30;
        public const int PerBadgeBonus = 1;
    }

    public static class Users
    {
        public const int EmailMaxLength = 256;
        public const int UserNameMinLength = 3;
        public const int UserNameMaxLength = 100;
        public const int PasswordMinLength = 6;
        public const int PasswordHashMaxLength = 500;
        public const int RoleMaxLength = 20;
        public const int BioMaxLength = 1000;
        public const int MinAge = 16;
        public const int MaxAge = 120;
        public const int SeedAdminAge = 30;
        public const int SystemUserAge = 0;
    }
}
