using PeopleOnDuty.src.Modules.TelegramBot;
using PeopleOnDuty.src.Modules.TelegramBot.Scenarios;
using Telegram.Bot.Types;

namespace PeopleOnDuty.src.Modules.TelegramBot.Handlers.UpdateHandler
{
    public class ScenarioRouter(IEnumerable<IScenario> _scenarios, PendingActionQueue _queue)
    {


        public void Enqueue(StateType state, Update update, UserState userState)
        {
            _queue.Enqueue(new PendingAction(state, update, userState));
        }
        public async Task RouteAsync(Update update, UserState state, CancellationToken ct)
        {
            // 1. Выполняем текущий сценарий
            var scenario = _scenarios.FirstOrDefault(s => s.CanHandle(state.CurrentState, update));
            if (scenario is not null)
                await scenario.HandleAsync(update, state, ct);

            // 2. Выполняем все задачи из очереди 
            while (_queue.TryDequeue(out var next) && next is not null)
            {
                var nextScenario = _scenarios.FirstOrDefault(s => s.CanHandle(next.State, next.Update));
                if (nextScenario is not null)
                    await nextScenario.HandleAsync(next.Update, next.UserState, ct);
            }
        }
    }

}
public class PendingAction
{
    public StateType State { get; }
    public Update Update { get; }
    public UserState UserState { get; }

    public PendingAction(StateType state, Update update, UserState userState)
    {
        State = state;
        Update = update;
        UserState = userState;
    }
}