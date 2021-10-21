import ReactDOM from "react-dom";
import { BrowserRouter } from "react-router-dom";
import { App } from "app";
import registerServiceWorker from "./registerServiceWorker";

const baseUrl = document
  .getElementsByTagName("base")[0]
  .getAttribute("href") as string;
const rootElement = document.getElementById("root");

ReactDOM.render(
  <BrowserRouter basename={baseUrl}>
    <App />
  </BrowserRouter>,
  rootElement
);

registerServiceWorker();
