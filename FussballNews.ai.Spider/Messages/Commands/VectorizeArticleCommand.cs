using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FussballNews.ai.Spider.Messages.Commands;

public record VectorizeArticleCommand(
    int ArticleId
);