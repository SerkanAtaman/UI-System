namespace SeroJob.UiSystem
{
    public static class UIDebugConstants
    {
        public const string OPENING_WINDOW = "Opening UI Window";
        public const string OPENING_WINDOWS = "Opening Multiple UI Windows";
        public const string CLOSING_WINDOW = "Closing UI Window";
        public const string CLOSING_WINDOWS = "Closing Multiple UI Windows";
        public const string PROCCESS_WORK_STARTED = "The Work of UI Proccess has been started";
        public const string PROCCESS_REWORK_STARTED = "The Rework of UI Proccess has been started";
        public const string PROCCESS_WORK_COMPLETED = "The Work of UI Proccess has been Completed";
        public const string PROCCESS_REWORK_COMPLETED = "The Rework of UI Proccess has been Completed";
        public const string PROCCESS_WORK_COMPLETED_IMMEDIATE = "The Work of UI Proccess has been Completed Immediately";
        public const string PROCCESS_REWORK_COMPLETED_IMMEDIATE = "The Rework of UI Proccess has been Completed Immediately";
        public const string APPEND_PROCCESS_TO_SEQUENCE = "Appending UIProccess to the Sequence";
        public const string JOIN_PROCCESS_TO_SEQUENCE = "Joining UIProccess to the Sequence";
        public const string NO_CONFLICT_WINDOW = "Can not find any conflicted windows";
        public const string CONTINUE_WORK_ON_COLLECTION = "Continuing to Working on Collection";
        public const string CONTINUE_REWORK_ON_COLLECTION = "Continuing to Reworking on Collection";
        public const string CONFLICTED_WINDOW_FOUND = "A Conflicted UI Window Found";

        public const string ARRAY_NULL_EMPTY = "The target array is null or empty";
        public const string COMMAND_NOT_RECOGNIZED = "Failed to recognize requested command";
        public const string PROCCESS_NOT_WORKABLE = "You are trying to start working on a UIProccess but it is not an unworked proccess!. That is not allowed";
        public const string PROCCESS_NOT_REWORKABLE = "You are trying to start reworking on a UIProccess but it is not an worked proccess!. That is not allowed";

        public const string MULTIPLE_WINDOWS_CONFLICT = "Multiple windows conflicts with each other";
        public const string SEQUENCE_EMPTY = "You are trying to start working a null or empty UIProccess Sequence";
        public const string WORKING_EMPTY_PROCCESS = "You are trying to start working a null or empty UIProccess. Immediately compliting it";
        public const string REWORKING_EMPTY_PROCCESS = "You are trying to start reworking a null or empty UIProccess. Immediately compliting it";
    }
}