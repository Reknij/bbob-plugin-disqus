using Bbob.Plugin;

namespace bbob_plugin_disqus;

[PluginCondition("*", PluginOrder = PluginOrder.BeforeMe)]
public class Class1 : IPlugin
{
    public Class1()
    {
        PluginHelper.registerCustomCommand("config", (args)=>
        {
            if (args.Length == 2)
            {
                PluginHelper.getPluginJsonConfig<MyConfig>(out var tar);
                MyConfig config = tar ?? new MyConfig();
                switch (args[0])
                {
                    case "shortName":
                        config.shortName = args[1];
                        break;

                    default: break;
                }
                PluginHelper.savePluginJsonConfig(config);
                PluginHelper.printConsole("Success save config.");
            }
            else
            {
                PluginHelper.printConsole("Please enter config name and value!");
            }
        });
    }
    public void GenerateCommand(string filePath, GenerationStage stage)
    {
        if (stage == GenerationStage.FinalProcess)
        {
            PluginHelper.getRegisteredObject<dynamic>("article", out dynamic? article);
            if (article == null) return;
            string identifier = filePath.Replace(PluginHelper.CurrentDirectory, "");
            if (GetDisqusScript(identifier, out string script))
            {
                article.contentParsed +=$"\n\n{script}";
            }
        }
    }

    private bool GetDisqusScript(string identifier, out string script)
    {
        script = @"<div id='disqus_thread'></div>
<script>
    var disqus_config = function () {
    // this.page.url = PAGE_URL;  // Replace PAGE_URL with your page's canonical URL variable
    this.page.identifier = PAGE_IDENTIFIER; // Replace PAGE_IDENTIFIER with your page's unique identifier variable
    };
    (function() { // DON'T EDIT BELOW THIS LINE
    var d = document, s = d.createElement('script');
    s.src = 'https://SHORT_NAME.disqus.com/embed.js';
    s.setAttribute('data-timestamp', +new Date());
    (d.head || d.body).appendChild(s);
    })();
</script>
<noscript>Please enable JavaScript to view the <a href='https://disqus.com/?ref_noscript'>comments powered by Disqus.</a></noscript>".Replace('\'', '\"');
        PluginHelper.getPluginJsonConfig<MyConfig>(out var tar);
        MyConfig config = tar ?? new MyConfig();
        if (config.shortName == null)
        {
            PluginHelper.ExecutingCommandResult = new CommandResult("Please enter your shortName! Or disable bbob-plugin-disqus.", CommandOperation.Stop);
            return false;
        }
        script = script.Replace("SHORT_NAME", config.shortName).Replace("PAGE_IDENTIFIER", setJsString(identifier));
        return true;
    }

    private string setJsString(string str) => $"\"{str}\"";

    public class MyConfig
    {
        public string? shortName{get;set;}
    }
}
