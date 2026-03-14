using System.Collections.Generic;

namespace Game
{
    // This selector chooses the correct quit strategy for the effective platform.
    // It keeps strategy lookup out of the button view so the view only coordinates user interaction.
    // This fits the project because the strategy set is small and does not need a heavier factory layer.
    public class EndGameQuitStrategySelector
    {
        private readonly Dictionary<Platforms, IEndGameQuitStrategy> _strategy_table;
        private readonly IPlatformSelectionResolver _platform_selection_resolver;

        public EndGameQuitStrategySelector(
            List<IEndGameQuitStrategy> quit_strategies,
            IPlatformSelectionResolver platform_selection_resolver)
        {
            _strategy_table = new Dictionary<Platforms, IEndGameQuitStrategy>();
            _platform_selection_resolver = platform_selection_resolver;

            foreach (IEndGameQuitStrategy quit_strategy in quit_strategies)
            {
                if (_strategy_table.ContainsKey(quit_strategy.target_platform))
                {
                    continue;
                }

                _strategy_table.Add(quit_strategy.target_platform, quit_strategy);
            }
        }

        public IEndGameQuitStrategy Resolve()
        {
            Platforms resolved_platform = _platform_selection_resolver.ResolvePlatform();

            if (_strategy_table.TryGetValue(resolved_platform, out IEndGameQuitStrategy quit_strategy))
            {
                return quit_strategy;
            }

            return _strategy_table[Platforms.PC];
        }
    }
}
