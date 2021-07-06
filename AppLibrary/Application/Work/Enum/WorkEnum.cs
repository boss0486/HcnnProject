namespace WebCore.ENM
{

    public class WorkEnum
    {

        public enum State
        {
            None = 0,
            Assigned = 1,
            Reception = 2,
            Processing = 3,
            Pause = 4,
            Completed = 5
        }

        public enum ReceptionType
        {
            None = 0,
            OutSite = 1,
            Department = 2,
            UserOfDepartment = 3,

        }

        public enum AssignType
        {
            None = 0,
            Direct = 1,
            IsChild = 2,
        }
        public enum UserType
        {
            None = 0,
            UserExecute = 1,
            UserFollows = 2,
        }
    }
}