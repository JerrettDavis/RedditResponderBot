
namespace Bot.Service.Application.Comments.Models
{
    public static class CommentStoreConstants
    {
        public static string OldCommentsQueue => "OldComments";
        public static string NewCommentsQueue => "Newcomments";
        public static string AddedQueue => "Added";
        public static string RemovedQueue => "Removed";
    }
}