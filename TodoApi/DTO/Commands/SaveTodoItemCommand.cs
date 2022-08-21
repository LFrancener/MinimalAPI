namespace TodoApi.DTO.Commands
{
    public class SaveOrUpdateTodoItemCommand
    {
        public string? Name { get; set; }
        public bool IsComplete { get; set; }
    }
}
