<template>
    <b-nav tabs fill class="mt-2">
        <b-nav-item v-for="i in this.menuItems" :key="i.name" :active="i.active" :disabled="i.disabled" :to="i.to">
            {{ i.name }}
        </b-nav-item>
    </b-nav>
</template>

<script>
export default {
    name: "Menu",
    data() {
        return {
            menuItems: [
                {
                    name: "General",
                    to: "/admin",
                    active: false,
                    disabled: false
                },
                {
                    name: "Users",
                    to: "/admin/users",
                    active: false,
                    disabled: false
                },
                {
                    name: "Groups",
                    to: "/admin/groups",
                    active: false,
                    disabled: false
                },
                {
                    name: "Plugins",
                    to: "/admin/plugins",
                    active: false,
                    disabled: true
                },
            ]
        }
    },
    props: {
        activeMenuItem: String
    },
    watch: {
        activeMenuItem(newValue, oldValue) {
            this.menuItems.find(x => x.name == newValue).active = true;
            this.menuItems.find(x => x.name == oldValue).active = false;
        },
    },
    mounted: function(){
        this.menuItems.find(x => x.name == this.activeMenuItem).active = true;
    }
}
</script>

<style>
</style>>