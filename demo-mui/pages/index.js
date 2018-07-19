import Head from "next/head";
import { Fragment } from "react";
import AppLayout from "../layouts/app-layout";
import Button from "@material-ui/core/Button";

export default () =>
  <AppLayout classes={{}}>
    <div>Hello World.</div>
  </AppLayout>;

// export default () =>
//   <Fragment>
//     <Head>
//       <link rel="stylesheet" href="https://fonts.googleapis.com/css?family=Roboto:300,400,500"/>
//       <link rel="stylesheet" href="https://fonts.googleapis.com/icon?family=Material+Icons"/>
//     </Head>
//     <div>Welcome to next.js!!!</div>
//     <Button variant="contained" color="primary">
//       Hello World
//     </Button>
//   </Fragment>
