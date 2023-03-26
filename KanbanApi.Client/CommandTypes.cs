namespace KanbanApi.Client;

public enum CommandTypes
{
    UpdateCardCommand = 0,
    RemoveCardCommand = 1,
    UpdateLaneCommand = 2,
    RemoveLaneCommand = 3,
    ExcludeChildCommand = 4,
    AppendChildCommand = 5,
    InsertBeforeCommand = 6,
    InsertAfterCommand = 7,
    LayoutBoardCommand = 8,
    LayoutLaneCommand = 9,
    DescribeBoardCommand = 10,
    DescribeLaneCommand = 11,
    DescribeCardCommand = 12,
    UpdateBoardCommand = 13,
    StateBoardCommand = 14
}