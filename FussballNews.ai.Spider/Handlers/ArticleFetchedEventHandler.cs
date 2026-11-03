using FussballNews.ai.Spider.Messages.Commands;
using FussballNews.ai.Spider.Messages.Events;
using Rebus.Bus;
using Rebus.Handlers;

namespace FussballNews.ai.Spider.Handlers;

public class ArticleFetchedEventHandler(
    IBus bus
) : IHandleMessages<ArticleFetchedEvent>
{
    public async Task Handle(ArticleFetchedEvent message)
    {
        await bus.Publish(new VectorizeArticleCommand(
            ArticleId: message.ArticleId
        ));
    }
}