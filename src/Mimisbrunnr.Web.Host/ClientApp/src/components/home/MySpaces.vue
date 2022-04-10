<template>
    <b-list-group>
        <b-list-group-item button><b-skeleton animation="fade" width="70%"></b-skeleton></b-list-group-item>
        <b-list-group-item button><b-skeleton animation="fade" width="40%"></b-skeleton></b-list-group-item>
        <b-list-group-item button disabled><b-skeleton animation="fade" width="80%"></b-skeleton></b-list-group-item>
        <b-list-group-item button><b-skeleton animation="fade" width="20%"></b-skeleton></b-list-group-item>
        <b-list-group-item button><b-skeleton animation="fade" width="25%"></b-skeleton></b-list-group-item>
    </b-list-group>
</template>

<script>
export default {
    name: "MySpaces"
}
</script>

<style>

</style>