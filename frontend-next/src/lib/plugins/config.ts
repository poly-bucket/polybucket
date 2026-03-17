import { loadPlugins, freezePlugins } from "./registry";
import { corePlugin } from "./core-plugin";

loadPlugins([corePlugin]);
freezePlugins();
