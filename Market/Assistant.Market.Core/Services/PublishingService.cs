﻿namespace Assistant.Market.Core.Services;

using System.Text.RegularExpressions;
using Assistant.Market.Core.Models;
using Common.Core.Utils;
using Helper.Core.Domain;
using Helper.Core.Utils;
using Microsoft.Extensions.Logging;
using OptionChain = Assistant.Market.Core.Models.OptionChain;
using Stock = Assistant.Market.Core.Models.Stock;

public class PublishingService : IPublishingService
{
    private static readonly IDictionary<string, string> FontStyle = new Dictionary<string, string>
    {
        ["fontWeight"] = "500",
        ["fontStyle"] = "normal"
    };

    private static readonly IDictionary<string, string> EvenRowStyle = new Dictionary<string, string>
    {
        ["backgroundColor"] = "rgba(0,0,0,.03)"
    };
    
    private static readonly IDictionary<string, string> SmallFontStyle = new Dictionary<string, string>
    {
        ["fontSize"] = "50%"
    };

    private static readonly IDictionary<string, string> WideCellStyle = new Dictionary<string, string>
    {
        ["width"] = "4.5rem"
    };

    private static readonly IDictionary<string, string> ContentStyle = new Dictionary<string, string>
    {
        ["width"] = "2.5rem"
    };

    private static readonly IDictionary<string, string> ContentHeaderStyle =
        RenderUtils.MergeStyle(FontStyle, ContentStyle);

    private const string MarketData = "Market Data";
    private const string OpenInterest = "Open Interest";
    private const int ChunkSize = 10;

    private readonly IStockService stockService;
    private readonly IOptionService optionService;
    private readonly IKanbanService kanbanService;
    private readonly ILogger<PublishingService> logger;

    public PublishingService(IStockService stockService, IOptionService optionService, IKanbanService kanbanService,
        ILogger<PublishingService> logger)
    {
        this.stockService = stockService;
        this.optionService = optionService;
        this.kanbanService = kanbanService;
        this.logger = logger;
    }

    public async Task PublishAsync()
    {
        this.logger.LogInformation("{Method}", nameof(this.PublishAsync));

        var stocks = await this.stockService.FindAllAsync();
        var map = stocks.ToDictionary(stock => stock.Ticker);
        var counter = 1;

        foreach (var chunk in map.Values.OrderBy(stock => stock.Ticker).Chunk(ChunkSize))
        {
            await this.PublishAsync(counter++, chunk);
        }
    }

    public async Task PublishOpenInterestAsync()
    {
        this.logger.LogInformation("{Method}", nameof(this.PublishOpenInterestAsync));

        var today = DateTimeUtils.TodayUtc();
        
        if (today.DayOfWeek == DayOfWeek.Saturday)
        {
            today = today.AddDays(-1);
        }
        else if (today.DayOfWeek == DayOfWeek.Sunday)
        {
            today = today.AddDays(-2);
        }

        var prefix = $"{OpenInterest} ({today.DayOfWeek})";

        var boards = await this.kanbanService.FindBoardsAsync();
        var board = boards.FirstOrDefault(board => board.Name.StartsWith(prefix));

        var now = DateTime.UtcNow;
        var name = $"{prefix} {now.ToShortDateString()} {now.ToShortTimeString()}";

        if (board != null)
        {
            board.Name = name;
            board.Description = "Calculation...";
            await this.kanbanService.UpdateBoardAsync(board);
        }
        else
        {
            board = await this.kanbanService.CreateBoardAsync(new Board
                { Name = name, Description = "Calculation..." });
        }

        await this.PublishOpenInterestAsync(board, today.AddHours(-4), new OpenInterestFilter
        {
            PublishIncrease = true,
            PublishDecrease = false,
            Top = 20,
            MinPercent = 20m
        });
    }

    private async Task PublishOpenInterestAsync(Board board, DateTime today, OpenInterestFilter filter)
    {
        const int maxDescTickers = 90;

        try
        {
            await this.kanbanService.SetBoardLoadingStateAsync(board.Id);

            await this.RemoveBoardLanesAsync(board);

            var stocks = await this.stockService.FindAllAsync();

            var stockMap = stocks.ToDictionary(stock => stock.Ticker);

            var dictionary = new Dictionary<string, int>();

            foreach (var ticker in stockMap.Keys.OrderBy(t => t))
            {
                var count = await this.optionService.FindChangesCountAsync(ticker, () => today);

                if (count > 0)
                {
                    dictionary.Add(ticker, count);
                }
            }

            var lane = await this.kanbanService.CreateCardLaneAsync(board.Id, board.Id, new Lane
            {
                Name = OpenInterest,
                Description = $"{filter.AsDescription()}. Companies ordered by the absolute change of OI in contracts, from high to low value"
            });

            var list = new List<Tuple<decimal, Card>>();

            foreach (var pair in dictionary.OrderByDescending(p => p.Value))
            {
                var props = new List<string>();

                var price = stockMap[pair.Key].Last;

                var stock = Helper.Core.Domain.Stock.From(pair.Key);
                stock.Price = MarketPrice.From(price);

                var propPrice = RenderUtils.PairToContent(
                    RenderUtils.PropToContent("Price"),
                    RenderUtils.PropToContent($"{FormatUtils.FormatPrice(price)}", WideCellStyle));

                props.Add(propPrice);

                var change = decimal.Zero;
                var percentChange = decimal.Zero;
                
                if (filter.PublishDecrease)
                {
                    var min = await this.optionService.FindOpenInterestChangeMinAsync(pair.Key, () => today);
                    var percentMin = await this.optionService.FindOpenInterestChangePercentMinAsync(pair.Key, () => today);

                    if (min > decimal.Zero)
                    {
                        min = decimal.Zero;
                        percentMin = decimal.Zero;
                    }

                    var propMin = RenderUtils.PairToContent(
                        RenderUtils.PropToContent("OI max \u2193", SmallFontStyle),
                        RenderUtils.PropToContent(
                            $"{FormatUtils.FormatAbsNumber(min)} ({FormatUtils.FormatAbsPercent(percentMin, 2)})",
                            GetNumberStyle(min)));
                    
                    props.Add(propMin);

                    change = Math.Max(Math.Abs(min), change);
                    percentChange = Math.Max(Math.Abs(percentMin), percentChange);
                }

                if (filter.PublishIncrease)
                {
                    var max = await this.optionService.FindOpenInterestChangeMaxAsync(pair.Key, () => today);
                    var percentMax = await this.optionService.FindOpenInterestChangePercentMaxAsync(pair.Key, () => today);
                    
                    if (max < decimal.Zero)
                    {
                        max = decimal.Zero;
                        percentMax = decimal.Zero;
                    }

                    var propMax = RenderUtils.PairToContent(
                        RenderUtils.PropToContent("OI max \u2191", SmallFontStyle),
                        RenderUtils.PropToContent(
                            $"{FormatUtils.FormatAbsNumber(max)} ({FormatUtils.FormatAbsPercent(percentMax, 2)})",
                            GetNumberStyle(max)));

                    props.Add(propMax);
                    
                    change = Math.Max(Math.Abs(max), change);
                    percentChange = Math.Max(Math.Abs(percentMax), percentChange);
                }

                var tops = await this.optionService.FindTopsAsync(pair.Key, filter.Top, () => today);

                var groups = tops
                    .Where(top => filter.PublishDecrease || top.OpenInterestChange >= decimal.Zero)
                    .Where(top => filter.PublishIncrease || top.OpenInterestChange < decimal.Zero)
                    .Where(top => CalculationUtils.Percent(Math.Abs(top.OpenInterestChange) / change) >= filter.MinPercent)
                    .GroupBy(top => OptionUtils.ParseExpiration(top.OptionTicker))
                    .OrderByDescending(group => group.Sum(top => Math.Abs(top.OpenInterestChange)));

                var oddRow = false;
                
                foreach (var group in groups)
                {
                    var expiration = Expiration.FromYYYYMMDD(OptionUtils.GetExpiration(group.First().OptionTicker));
                    var groupLabel = $"{FormatUtils.FormatExpiration(expiration.AsDate())}";
                    var groupValue = $"{expiration.DaysTillExpiration}d";
                    var groupProp = RenderUtils.PairToContent(RenderUtils.PropToContent(groupLabel, oddRow ? null : EvenRowStyle), RenderUtils.PropToContent(groupValue, oddRow ? WideCellStyle : RenderUtils.MergeStyle(EvenRowStyle, WideCellStyle)));
                    
                    props.Add(groupProp);

                    var labelStyle = oddRow ? SmallFontStyle : RenderUtils.MergeStyle(EvenRowStyle, SmallFontStyle);

                    foreach (var top in group)
                    {
                        var strike = OptionUtils.GetStrike(top.OptionTicker);
                        var option = OptionUtils.IsCall(top.OptionTicker) ? StockOption.Call(stock, strike, expiration) : StockOption.Put(stock, strike, expiration);
                        var op = option.Sell(top.Last);
                        var label = $"{OptionUtils.GetSide(top.OptionTicker)}${strike} {FormatUtils.FormatAbsPercent(CalculationUtils.Percent(op.Roi), 0)}/{FormatUtils.FormatAbsPercent(CalculationUtils.Percent(op.AnnualRoi), 0)}";
                        var value = $"{FormatUtils.FormatAbsNumber(top.OpenInterestChange)} ({FormatUtils.FormatAbsPercent(top.OpenInterestChangePercent, 2)}) {FormatUtils.FormatPrice(top.Last)}";
                        var valueStyle = oddRow ? GetNumberStyle(top.OpenInterestChange) : RenderUtils.MergeStyle(EvenRowStyle, GetNumberStyle(top.OpenInterestChange));
                        var prop = RenderUtils.PairToContent(RenderUtils.PropToContent(label, labelStyle), RenderUtils.PropToContent(value, valueStyle));
                        props.Add(prop);
                    }

                    oddRow = !oddRow;
                }

                var desc = props.Aggregate((curr, i) => $"{curr}, {i}");
                    
                var card = new Card
                {
                    Name = $"{pair.Key} ({pair.Value})",
                    Description = $"[{desc}]"
                };

                list.Add(new Tuple<decimal, Card>(change, card));
            }

            var counter = 0;

            board.Description = string.Empty;

            foreach (var pair in list.OrderByDescending(pair => pair.Item1))
            {
                try
                {
                    await this.kanbanService.CreateCardAsync(board.Id, lane.Id, pair.Item2);
                }
                catch (Exception e)
                {
                    this.logger.LogError(e, "Failed to create card {Name} in board with id {Id}", pair.Item2.Name,
                        board.Id);
                }

                if (counter++ < maxDescTickers)
                {
                    board.Description += (counter == 1 ? string.Empty : ", ") + pair.Item2.Name.Split(' ')[0];

                    if (counter == maxDescTickers)
                    {
                        board.Description += "...";
                    }
                }
            }

            await this.kanbanService.UpdateBoardAsync(board);
        }
        catch (Exception e)
        {
            this.logger.LogError(e, "Failed to publish open interest data for {Board} with {Content}", board.Name,
                board.Description);
        }
        finally
        {
            await this.kanbanService.ResetBoardStateAsync(board.Id);
        }
    }

    private static IEnumerable<Tuple<string, string>> YieldNumberStyle(decimal number)
    {
        switch (number)
        {
            case < decimal.Zero:
                yield return RenderUtils.Red;
                break;
            case > decimal.Zero:
                yield return RenderUtils.Green;
                break;
        }
    }

    private static IDictionary<string, string> GetNumberStyle(decimal number)
    {
        return RenderUtils.MergeStyle(SmallFontStyle, WideCellStyle, RenderUtils.CreateStyle(YieldNumberStyle(number).ToArray()));
    }

    private async Task RemoveBoardLanesAsync(Board board)
    {
        var lanes = await this.kanbanService.FindLanesAsync(board.Id);

        foreach (var laneId in lanes.Select(lane => lane.Id))
        {
            var cards = await this.kanbanService.FindCardsAsync(board.Id, laneId);

            foreach (var cardId in cards.Select(card => card.Id))
            {
                await this.kanbanService.RemoveCardAsync(board.Id, cardId, laneId);
            }
            
            await this.kanbanService.RemoveLaneAsync(board.Id, laneId);
        }
    }

    private async Task PublishAsync(int chunkNo, Stock[] chunk)
    {
        var key = $"{MarketData} {chunkNo}";
        var name = $"{key} ({chunk.Length})";
        var description = chunk.Select(item => item.Ticker).Aggregate((curr, i) => $"{curr}, {i}");

        const string pattern = @"\(\d+\)";
        var boards = await this.kanbanService.FindBoardsAsync();
        var board = boards
            .Where(board => board.Name.StartsWith(MarketData))
            .FirstOrDefault(board => Regex.IsMatch(board.Name, $"{key} {pattern}"));

        if (board != null)
        {
            board.Name = name;
            board.Description = description;

            await this.kanbanService.UpdateBoardAsync(board);
        }
        else
        {
            board = await this.kanbanService.CreateBoardAsync(new Board { Name = name, Description = description });
        }

        await this.PublishAsync(board, chunk);
    }

    private async Task PublishAsync(Board board, Stock[] chunk)
    {
        try
        {
            await this.kanbanService.SetBoardLoadingStateAsync(board.Id);

            var stockLanes = await this.PublishTickersAsync(board, chunk);

            await this.PublishExpirationsAsync(board, chunk, stockLanes);
        }
        catch (Exception e)
        {
            this.logger.LogError(e, "Failed to publish market data for {Board} with {Content}", board.Name,
                board.Description);
        }
        finally
        {
            await this.kanbanService.ResetBoardStateAsync(board.Id);
        }
    }

    private async Task<IDictionary<string, Lane>> PublishTickersAsync(Board board, Stock[] chunk)
    {
        var lanes = await this.kanbanService
            .FindBoardLanesAsync(board.Id);

        var stockLanes = lanes.ToDictionary(lane => lane.Name, lane => lane);

        foreach (var stock in chunk)
        {
            var description = stock.LastRefresh != DateTime.UnixEpoch
                ? $"${Math.Round(stock.Last, 2)}, {stock.LastRefresh.ToShortDateString()} {stock.LastRefresh.ToShortTimeString()}"
                : "n/a";

            if (!stockLanes.ContainsKey(stock.Ticker))
            {
                var lane = await this.kanbanService
                    .CreateBoardLaneAsync(board.Id, new Lane { Name = stock.Ticker, Description = description });

                stockLanes.Add(stock.Ticker, lane);
            }
            else
            {
                stockLanes[stock.Ticker].Description = description;

                await this.kanbanService
                    .UpdateBoardLaneAsync(board.Id, stockLanes[stock.Ticker]);
            }
        }

        var tickers = chunk.Select(i => i.Ticker).ToHashSet();

        // remove ticker lanes, which are not included in chunk
        foreach (var pair in stockLanes.Where(pair => !tickers.Contains(pair.Key)))
        {
            await this.kanbanService.RemoveLaneAsync(board.Id, pair.Value.Id);
        }

        return stockLanes;
    }

    private async Task PublishExpirationsAsync(Board board, Stock[] chunk, IDictionary<string, Lane> stockLanes)
    {
        foreach (var stock in chunk.Where(i => stockLanes.ContainsKey(i.Ticker)))
        {
            var optionChain = await this.optionService.FindAsync(stock.Ticker);

            var changeChain = await this.optionService.FindChangeAsync(stock.Ticker);

            var stockLane = stockLanes[stock.Ticker];

            var lanes = await this.kanbanService.FindLanesAsync(board.Id, stockLane.Id);

            var expirationLanes = lanes.ToDictionary(l => l.Name);

            foreach (var expiration in optionChain.Expirations.Keys.OrderBy(i => i))
            {
                await this.PublishExpirationAsync(board, stockLane, optionChain, changeChain, expiration,
                    expirationLanes);
            }

            var optionChainExpirations = optionChain.Expirations.Keys.ToHashSet();

            // remove expiration lanes, which are not included in chain
            foreach (var pair in expirationLanes.Where(pair => !optionChainExpirations.Contains(pair.Key)))
            {
                await this.kanbanService.RemoveLaneAsync(board.Id, pair.Value.Id);
            }
        }
    }

    private const string CallsCardName = "CALLS";
    private const string PutsCardName = "PUTS";

    private async Task PublishExpirationAsync(Board board, Lane stockLane, OptionChain chain, OptionChain change,
        string expiration,
        IDictionary<string, Lane> expirationLanes)
    {
        if (!expirationLanes.ContainsKey(expiration))
        {
            var lane = await this.kanbanService.CreateCardLaneAsync(board.Id, stockLane.Id, new Lane
            {
                Name = expiration
            });

            expirationLanes.Add(expiration, lane);
        }

        var expirationLane = expirationLanes[expiration];

        var cards = await this.kanbanService.FindCardsAsync(board.Id, expirationLane.Id);

        var cardsMap = cards.ToDictionary(c => c.Name);

        var changeExpiration = GetChangeExpiration(change, expiration);

        var callDesc = CallsContent(chain.Expirations[expiration], changeExpiration);

        if (!cardsMap.ContainsKey(CallsCardName))
        {
            await this.kanbanService
                .CreateCardAsync(board.Id, expirationLane.Id,
                    new Card { Name = CallsCardName, Description = callDesc });
        }
        else
        {
            cardsMap[CallsCardName].Description = callDesc;

            await this.kanbanService.UpdateCardAsync(board.Id, cardsMap[CallsCardName]);
        }

        var putDesc = PutsContent(chain.Expirations[expiration], changeExpiration);

        if (!cardsMap.ContainsKey(PutsCardName))
        {
            await this.kanbanService
                .CreateCardAsync(board.Id, expirationLane.Id, new Card { Name = PutsCardName, Description = putDesc });
        }
        else
        {
            cardsMap[PutsCardName].Description = putDesc;

            await this.kanbanService.UpdateCardAsync(board.Id, cardsMap[PutsCardName]);
        }
    }

    private static OptionExpiration? GetChangeExpiration(OptionChain change, string expiration)
    {
        if (!change.Expirations.TryGetValue(expiration, out var data))
        {
            return null;
        }

        return data.LastRefresh >= DateTimeUtils.TodayUtc() ? data : null;
    }

    private static OptionContract? GetCallContract(decimal strike, OptionExpiration? change)
    {
        if (change == null || !change.Contracts.TryGetValue(strike, out var contracts))
        {
            return null;
        }

        return contracts.Call;
    }

    private static string OptionsContent(
        OptionExpiration expiration, 
        OptionExpiration? change,
        Func<OptionContracts, OptionContract> sideFn, 
        Func<decimal, OptionExpiration?, OptionContract> contractFn)
    {
        var strikesWithContracts = expiration.Contracts
            .Where(pair => sideFn(pair.Value) != null)
            .OrderBy(pair => pair.Key)
            .ToList();

        var priceTuples = strikesWithContracts.Select(pair => PriceToContent(pair.Key, sideFn(pair.Value))).ToList();
        if (priceTuples.Count > 0)
        {
            priceTuples.Insert(0, new Row
            {
                Key = new Item
                {
                    Text = "Strike",
                    Style = FontStyle
                },
                Values = new[]
                {
                    new Item
                    {
                        Text = "Bid",
                        Style = ContentHeaderStyle
                    },
                    new Item
                    {
                        Text = "Ask",
                        Style = ContentHeaderStyle
                    }
                }
            });
        }

        if (change != null)
        {
            var openInterestContracts = strikesWithContracts
                .Select(pair => new KeyValuePair<decimal, OptionContract>(pair.Key, contractFn(pair.Key, change)))
                .Where(pair => pair.Value != null && pair.Value.OI != decimal.Zero)
                .OrderBy(pair => pair.Key)
                .Select(pair => OpenInterestToContent(pair.Key, pair.Value))
                .ToList();

            if (openInterestContracts.Count > 0)
            {
                openInterestContracts.Insert(0, new Row
                {
                    Key = new Item
                    {
                        Text = "Space",
                        Style = RenderUtils.CreateStyle(new Tuple<string, string>("color", "transparent"))
                    },
                    Values = new[]
                    {
                        new Item
                        {
                            Text = "",
                            Style = ContentHeaderStyle
                        },
                        new Item
                        {
                            Text = "",
                            Style = ContentHeaderStyle
                        }
                    }
                });

                openInterestContracts.Insert(1, new Row
                {
                    Key = new Item
                    {
                        Text = "Strike",
                        Style = FontStyle
                    },
                    Values = new[]
                    {
                        new Item
                        {
                            Text = "Oi\u0394#",
                            Style = ContentHeaderStyle
                        },
                        new Item
                        {
                            Text = "Oi\u0394%",
                            Style = ContentHeaderStyle
                        }
                    }
                });

                priceTuples = priceTuples.Union(openInterestContracts).ToList();
            }
        }

        return RowsToContent(priceTuples);
    }

    private static string CallsContent(OptionExpiration expiration, OptionExpiration? change)
    {
        return OptionsContent(
            expiration, 
            change, 
            contracts => contracts.Call,
            (strike, exp) => GetCallContract(strike, exp));
    }

    private static OptionContract? GetPutContract(decimal strike, OptionExpiration? change)
    {
        if (change == null || !change.Contracts.TryGetValue(strike, out var contracts))
        {
            return null;
        }

        return contracts.Put;
    }

    private static string PutsContent(OptionExpiration expiration, OptionExpiration? change)
    {
        return OptionsContent(
            expiration, 
            change, 
            contracts => contracts.Put,
            (strike, exp) => GetPutContract(strike, exp));
    }

    private static string DecimalToContent(decimal? value)
    {
        return FormatUtils.FormatNumber(value, 2);
    }

    private static Row PriceToContent(decimal strike, OptionContract price)
    {
        return new Row
        {
            Key = new Item
            {
                Text = DecimalToContent(strike)
            },

            Values = PriceToContent(price)
        };
    }

    private static IEnumerable<Item> PriceToContent(OptionContract price)
    {
        yield return new Item
        {
            Text = FormatUtils.FormatPrice(price.Bid),
            Style = ContentStyle
        };

        yield return new Item
        {
            Text = FormatUtils.FormatPrice(price.Ask),
            Style = ContentStyle
        };
    }

    private static Row OpenInterestToContent(decimal strike, OptionContract contract)
    {
        return new Row
        {
            Key = new Item
            {
                Text = DecimalToContent(strike)
            },

            Values = OpenInterestToContent(contract)
        };
    }

    private static IEnumerable<Item> OpenInterestToContent(OptionContract contract)
    {
        var style = RenderUtils.MergeStyle(ContentStyle, contract.OI < decimal.Zero ? RenderUtils.RedStyle : RenderUtils.GreenStyle);

        yield return new Item
        {
            Text = FormatUtils.FormatAbsNumber(contract.OI),
            Style = style
        };

        yield return new Item
        {
            Text = FormatUtils.FormatAbsPercent(contract.Vol),
            Style = style
        };
    }

    private static string RowsToContent(IEnumerable<Row> rows)
    {
        var list = rows.ToList();
        if (list.Count == 0)
        {
            return string.Empty;
        }

        var body = list
            .Select(row => RenderUtils.PairToContent(RenderUtils.PropToContent(row.Key.Text, row.Key.Style),
                RenderUtils.PropsToContent(row.Values.Select(item =>
                    RenderUtils.PropToContent(item.Text, item.Style)))))
            .Aggregate((curr, x) => $"{curr},{x}");

        return $"[{body}]";
    }
}

internal class Row
{
    public Item Key { get; set; }

    public IEnumerable<Item> Values { get; set; }
}

internal class Item
{
    public string Text { get; set; }

    public IDictionary<string, string> Style { get; set; }
}