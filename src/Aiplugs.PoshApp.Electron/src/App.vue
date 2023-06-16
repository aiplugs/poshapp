<template>
  <v-app>
        <div class="app-layout">
            <section class="app-global-nav d-flex flex-column separator-vertical">
                <v-list dense nav>
                    <v-list-item to="/" active>
                        <v-list-item-icon>
                            <v-icon>mdi-hexagon-outline</v-icon>
                        </v-list-item-icon>
                        <v-list-item-content></v-list-item-content>
                    </v-list-item>
                    <v-list-item to="/scripts/">
                        <v-list-item-icon>
                            <v-icon>mdi-xml</v-icon>
                        </v-list-item-icon>
                        <v-list-item-content></v-list-item-content>
                    </v-list-item>
                    <v-list-item to="/repositories/">
                        <v-list-item-icon>
                            <v-icon>mdi-source-branch</v-icon>
                        </v-list-item-icon>
                        <v-list-item-content></v-list-item-content>
                    </v-list-item>
                </v-list>
                <v-spacer></v-spacer>
                <v-list dense nav>
                    <v-list-item to="/settings/">
                        <v-list-item-icon>
                            <v-icon>mdi-cog-outline</v-icon>
                        </v-list-item-icon>
                        <v-list-item-content></v-list-item-content>
                    </v-list-item>
                </v-list>
            </section>
            <section class="app-local-nav separator-vertical">
                <router-view name="nav"></router-view>
            </section>

            <main class="app-main">
                <router-view></router-view>
            </main>

            <v-footer class="app-footer pa-0 pl-2 pr-2" height="24px">
                <ProgressForUpdate></ProgressForUpdate>
                <span class="flex-grow-1"></span>
                <Prompt></Prompt>
                <PromptForChoice></PromptForChoice>
                <PromptForCredential></PromptForCredential>
                <Progress></Progress>
                <ProgressForGit></ProgressForGit>
                <LogForError></LogForError>
                <Log></Log>
            </v-footer>

            <Toast></Toast>
        </div>
        <UpdateNotice></UpdateNotice>
    </v-app>
</template>

<script>
import Toast from './components/UI/Toast'
import UpdateNotice from './components/UI/UpdateNotice'
import Prompt from './components/UI/Prompt'
import PromptForChoice from './components/UI/PromptForChoice'
import PromptForCredential from './components/UI/PromptForCredential'
import Progress from './components/UI/Progress'
import ProgressForGit from './components/UI/ProgressForGit'
import ProgressForUpdate from './components/UI/ProgressForUpdate'
import Log from './components/UI/Log'
import LogForError from './components/UI/LogForError'

export default {
  name: 'App',
  components: {
    Toast,
    UpdateNotice,
    Prompt,
    PromptForChoice,
    PromptForCredential,
    Progress,
    ProgressForGit,
    ProgressForUpdate,
    Log,
    LogForError,
  }
}
</script>

<style>
html, body {
    overflow: hidden;
}

.app-layout {
    width: 100vw;
    height: 100vh;
    display: grid;
    grid-template-columns: 56px 256px 1fr;
    grid-template-rows: 1fr 24px;
}

.app-global-nav {
    grid-column-start: 1;
    grid-column-end: 2;
    grid-row-start: 1;
    grid-row-end: 2;
    background: white;
}

.app-local-nav {
    grid-column-start: 2;
    grid-column-end: 3;
    grid-row-start: 1;
    grid-row-end: 2;
    background: white;
    overflow-y: auto;
}
.app-local-nav::-webkit-scrollbar {
    display: none;
}

.app-main {
    grid-column-start: 3;
    grid-column-end: 4;
    grid-row-start: 1;
    grid-row-end: 2;
    position:relative;
}

.app-footer {
    grid-column-start: 1;
    grid-column-end: 4;
    grid-row-start: 2;
    grid-row-end: 3;
}

.separator-vertical {
    border-right: 0.05rem solid #E0E0E0;
}

.separator-vertical .v-list {
    border-right-color: transparent;
}

.separator-horizontal {
    border-bottom: 0.5px solid #E0E0E0;
}

.main-content {
    position: absolute;
    width: 100%;
    height: 100%;
    overflow-y: hidden;
    overflow-x: hidden;
}

.main-content::-webkit-scrollbar {
    display: none;
}

.no-scrollbar::-webkit-scrollbar {
    display: none;
}

.scroll-content {
    overflow-y: auto;
    scroll-behavior: smooth;
    -webkit-overflow-scrolling: touch;
}

.scroll-content::-webkit-scrollbar,
.v-data-table__wrapper::-webkit-scrollbar {
    display: none ;
}

.bg {
    background: white;
}

@media (prefers-color-scheme: dark) {
  .app-global-nav,
  .app-local-nav {
      background: rgb(30, 30, 30);
  }
  .separator-horizontal,
  .separator-vertical {
      border-color: rgb(20, 20, 20);
  }
  .bg {
      background: rgb(30, 30, 30);
  }
}
</style>
