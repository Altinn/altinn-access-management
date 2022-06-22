# Front end for Altinn 3 Authorization

## Getting started 🚀

To get a full development environment with front and back end, check the [README file](../README.md) for the whole repo (basically, run `docker compose up`).

To run or build the front end standalone (without Docker), you'll need [Node](https://nodejs.org/) and [Yarn](https://yarnpkg.com/getting-started/install). Simply run `yarn` to install dependencies and then `yarn dev` to start the development environment.

(If you switch between the Docker-based and local development environments, always run `yarn` before `yarn dev` to ensure the dependencies are correct for your local environment.)

## Project organisation 🧩

This is a [Vite](https://vitejs.dev/)-based single-page app using [TypeScript](https://www.typescriptlang.org/). The main entry point is `/src/index.tsx`. You'll find React components under `/src/components/`, while code that deals with non-rendering functionality is in `/src/services/` or `/src/utils/` (only very simple stuff in the latter).

The basic file structure for components should be:

```
components\
  ComponentName\
    - ComponentName.tsx
    - ComponentName.test.tsx (unit tests)
    - index.ts (public interface for the component)
    - style.css (if needed)
    - SubComponent1.tsx (if needed)
    - SubComponent2.tsx (if needed)
```

🪄 You can run `yarn nc ComponentName` to create a new "ComponentName" component boilerplate like above.

## Coding conventions 👮‍♀️

### TypeScript

Use [PascalCase](https://techterms.com/definition/pascalcase) for both component and file names. We use ESLint with Prettier rules — you might want to add an ESLint plugin to your editor.

### CSS

Use [BEM naming convention](http://getbem.com/naming/) — this gives a pretty clear view of what parts are the "root" and what parts are the "children". This also helps you think about when a component grows too big, and should be split into smaller isolated parts.

Scalar values (sizes, colours…) should usually be references to design tokens exposed as CSS variables by the [Altinn Design System](https://github.com/Altinn/altinn-design-system). For example:

```css
.componentName {
  border: var(--border_width-standard) solid var(--color-success-calm);
  display: flex;
  padding: var(--space-base);
}
```

## Tests 🧨

Run `yarn test` to run [Vitest](https://vitest.dev) in watch mode.

Run `yarn coverage` to see pretty test coverage stats (interactive results get placed in the `coverage/` folder).

## Configuration ✅

At startup `services/config.ts` will pick up configuration from three possible sources, in order of priority:

- environment variables (these become static during the build)
- JSON embedded in a `<script>` tag with the correct ID, e.g.:

  ```html
  <script type="application/json" id="altinn3-app-config">
    {
      "backendApiUrl": "http://localhost:5117/api/"
    }
  </script>
  ```

- Hard-coded defaults

Use `getConfig()` to retrieve configuration at runtime. Currently configurable settings (please keep this list up to date):

| Env variable         | JSON          | Default                | Purpose                   |
|----------------------|---------------|------------------------|---------------------------|
| VITE_BACKEND_API_URL | backendApiUrl | current host + '/api/' | Base URL for BE API calls |
| VITE_DEFAULT_LOCALE  | defaultLocale | 'no'                   | Fallback locale           |

## Building and deploying 🚚

To create a distributable bundle, run `yarn build`. Environment variables set at build time will be baked into the bundle (e.g. `VITE_DEFAULT_LOCALE=en yarn build`). See the resulting `dist/index.html` for an example on how to load the build.

If the bundled files are to be served from a path other than the server root, you must pass the `--base=/path/to/folder/` argument to `yarn build`. The trailing slash is important.

TODO: 🙈 deployment

## Migration plan 🚧

This project aims to be a Single Page App (SPA) for the whole Profile page in the Altinn portal. Until it achieves that state, this app integrates into a larger Razor-template-driven page, rendered by the back end.

The idea is to migrate feature-by-feature parts of the page to the SPA, allowing continuous development of those features. This avoids a large "big bang" rewrite that migrates all features at once — that would prevent ongoing feature development until complete, or would require such work to be done in two codebases simultaneously (old and new).

### Method

The features managed by this app behave like a normal SPA, using `react-router`'s `<Link>` component for internal navigation. Features on the page still handled by the Razor back end use the standard request/response navigation of a Multi Page App (MPA), and non-React JavaScript that manipulates the DOM.

Given the mixed management of the DOM (React/Razor), this SPA makes use of [React Portals](https://reactjs.org/docs/portals.html) to maintain a single React VDOM tree.

The React DOM can be injected in disparate parts of the document by using `<div>` tags output by Razor with an appropriate ID for each feature — these serve as anchors for that part of the React-managed DOM. On initialisation of React, each feature's base component in injected into the appropriate anchor `<div>` via a React Portal.

```html
<!-- Razor template -->
<p>This is output by Razor</p>
<div id="altinn3-feature-x-root"></div>
<p>This is output by Razor</p>
<div id="altinn3-feature-y-root"></div>
<p>This is output by Razor</p>
```

```jsx
const featureXRoot = document.getElementById('altinn3-feature-x-root');
const featureYRoot = document.getElementById('altinn3-feature-y-root');

reactRoot.render(
  <>
    {featureXRoot && ReactDOM.createPortal(<FeatureX />, featureXRoot)}
    {featureYRoot && ReactDOM.createPortal(<FeatureY />, featureYRoot)}
  </>
);
```

The root of the React application itself is just another empty `<div>` on the page (created automatically on initialisation).

See `src/index.tsx` for the initialisation code.

### Objective

Once all features are migrated away from Razor, the SPA should manage the full DOM driven by `react-router` routing and the application's internal state — in short, a standard SPA architecture.

The Portals used during the transition period should then be removed and each feature base component should render directly within the React root.

```jsx
reactRoot.render(
  <>  
    <FeatureX />
    <FeatureY />
  </>
);
```
