using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FussballNews.ai.Spider.Messages.Events;

public record ArticleFetchedEvent(
    int ArticleId
);